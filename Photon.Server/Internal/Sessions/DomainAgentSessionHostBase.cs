using log4net;
using Photon.Communication;
using Photon.Framework.Domain;
using Photon.Framework.Extensions;
using Photon.Framework.Server;
using Photon.Library.TcpMessages;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Server.Internal.Sessions
{
    internal abstract class DomainAgentSessionHostBase : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(DomainAgentSessionHostBase));

        protected CancellationToken Token {get;}
        protected ServerAgent Agent {get;}
        public DomainAgentSessionClient SessionClient {get;}
        public MessageClient MessageClient {get;}
        public TaskRunnerManager Tasks {get;}

        public string AgentSessionId {get; protected set;}
        public string SessionClientId => SessionClient.Id;


        protected DomainAgentSessionHostBase(IServerSession sessionBase, ServerAgent agent, CancellationToken token)
        {
            this.Agent = agent;
            this.Token = token;

            Tasks = new TaskRunnerManager();

            SessionClient = new DomainAgentSessionClient();
            SessionClient.OnSessionBegin += SessionClient_OnSessionBegin;
            SessionClient.OnSessionRelease += SessionClient_OnSessionRelease;
            SessionClient.OnSessionRunTask += SessionClient_OnSessionRunTask;

            MessageClient = new MessageClient(PhotonServer.Instance.MessageRegistry);
            MessageClient.Transceiver.Context = sessionBase;

            MessageClient.ThreadException += MessageClient_OnThreadException;
        }

        public virtual void Dispose()
        {
            Tasks?.Dispose();
            SessionClient.Dispose();
            MessageClient?.Dispose();
        }

        public void Abort()
        {
            if (MessageClient.IsConnected && !string.IsNullOrEmpty(AgentSessionId)) {
                try {
                    OnCancelSession();
                }
                catch (Exception error) {
                    Log.Error($"Failed to cancel Agent Session '{AgentSessionId}'! {error.Message}");
                }
            }
        }

        protected abstract Task OnBeginSession(CancellationToken token);

        protected abstract Task OnReleaseSessionAsync(CancellationToken token);

        protected abstract void OnSessionOutput(string text);

        private void SessionClient_OnSessionBegin(RemoteTaskCompletionSource taskHandle)
        {
            Task.Run(() => ConnectToAgent(Token), Token)
                .ContinueWith(taskHandle.FromTask, Token);
        }

        private async Task ConnectToAgent(CancellationToken token)
        {
            var agentAddress = $"{Agent.TcpHost}:{Agent.TcpPort}";
            Log.Debug($"Connecting to TCP Agent '{agentAddress}'...");

            try {
                await MessageClient.ConnectAsync(Agent.TcpHost, Agent.TcpPort, Token);
                Log.Info($"Connected to TCP Agent '{agentAddress}'.");

                Log.Debug($"Performing TCP handshake... [{agentAddress}]");

                await ClientHandshake.Verify(MessageClient, token);

                Log.Info($"Handshake successful. [{agentAddress}].");
            }
            catch (Exception error) {
                Log.Error($"Failed to connect to TCP Agent '{agentAddress}'!", error);
                MessageClient.Dispose();
                throw;
            }

            await OnBeginSession(token);

            Tasks.Start();
        }

        private void OnCancelSession()
        {
            var message = new SessionCancelRequest {
                AgentSessionId = AgentSessionId,
            };

            MessageClient.SendOneWay(message);
        }

        private void SessionClient_OnSessionRelease(RemoteTaskCompletionSource taskHandle)
        {
            Task.Run(async () => {
                Tasks.Stop();

                if (MessageClient.IsConnected) {
                    if (!string.IsNullOrEmpty(AgentSessionId)) {
                        try {
                            await OnReleaseSessionAsync(Token);
                        }
                        catch (Exception error) {
                            Log.Error($"Failed to release Agent Session '{AgentSessionId}'! {error.Message}");
                        }
                    }

                    MessageClient.Disconnect();
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
