using log4net;
using Photon.Communication;
using Photon.Framework;
using Photon.Framework.Domain;
using Photon.Framework.Extensions;
using System;
using System.Runtime.Remoting.Lifetime;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Server.Internal.Sessions
{
    internal abstract class DomainAgentSessionHostBase : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(DomainAgentSessionHostBase));

        protected readonly CancellationToken Token;
        private readonly ServerAgentDefinition agent;
        private readonly ClientSponsor sponsor;
        public DomainAgentSessionClient SessionClient {get;}
        public MessageClient MessageClient {get;}

        public TaskRunnerManager Tasks {get;}
        public string AgentSessionId {get; protected set;}
        public string SessionClientId => SessionClient.Id;


        protected DomainAgentSessionHostBase(IServerSession sessionBase, ServerAgentDefinition agent, CancellationToken token)
        {
            this.agent = agent;
            this.Token = token;

            Tasks = new TaskRunnerManager();

            SessionClient = new DomainAgentSessionClient();
            SessionClient.OnSessionBegin += SessionClient_OnSessionBegin;
            SessionClient.OnSessionRelease += SessionClient_OnSessionRelease;
            SessionClient.OnSessionRunTask += SessionClient_OnSessionRunTask;

            sponsor = new ClientSponsor();
            sponsor.Register(SessionClient);

            MessageClient = new MessageClient(PhotonServer.Instance.MessageRegistry) {
                Context = sessionBase,
            };

            MessageClient.ThreadException += MessageClient_OnThreadException;
        }

        public virtual void Dispose()
        {
            Tasks?.Dispose();
            MessageClient?.Dispose();
            sponsor?.Close();
        }

        protected abstract Task OnBeginSession();

        protected abstract Task OnReleaseSessionAsync();

        protected abstract void OnSessionOutput(string text);

        private void SessionClient_OnSessionBegin(RemoteTaskCompletionSource taskHandle)
        {
            Task.Run(async () => {
                Log.Debug($"Connecting to TCP Agent '{agent.TcpHost}:{agent.TcpPort}'...");

                try {
                    await MessageClient.ConnectAsync(agent.TcpHost, agent.TcpPort, Token);
                    Log.Info($"Connected to TCP Agent '{agent.TcpHost}:{agent.TcpPort}'.");
                }
                catch (Exception error) {
                    Log.Error($"Failed to connect to TCP Agent '{agent.TcpHost}:{agent.TcpPort}'!", error);
                    throw;
                }

                await OnBeginSession();

                Tasks.Start();
                return (object)null;
            }, Token)
                .ContinueWith(taskHandle.FromTask, Token);
        }

        private void SessionClient_OnSessionRelease(RemoteTaskCompletionSource taskHandle)
        {
            Task.Run(async () => {
                Tasks.Stop();

                if (MessageClient.IsConnected) {
                    if (!string.IsNullOrEmpty(AgentSessionId)) {
                        try {
                            await OnReleaseSessionAsync();
                        }
                        catch (Exception error) {
                            Log.Error($"Failed to release Agent Session '{AgentSessionId}'! {error.Message}");
                        }

                    }

                    await MessageClient.DisconnectAsync();
                }
            }, Token)
                .ContinueWith(taskHandle.FromTask, Token);
        }

        private void SessionClient_OnSessionRunTask(string taskName, RemoteTaskCompletionSource taskHandle)
        {
            Task.Run(async () => {
                var runner = new TaskRunner(MessageClient, AgentSessionId);
                runner.OutputEvent += (o, e) => {
                    OnSessionOutput(e.Text);
                };

                Tasks.Add(runner);

                await runner.Run(taskName, Token);
            }, Token)
                .ContinueWith(taskHandle.FromTask, Token);
        }

        private void MessageClient_OnThreadException(object sender, UnhandledExceptionEventArgs e)
        {
            Log.Error("Unhandled TCP Message Error!", (Exception)e.ExceptionObject);
        }
    }
}
