using log4net;
using Photon.Communication;
using Photon.Framework.Domain;
using Photon.Framework.Extensions;
using Photon.Framework.Server;
using Photon.Library.TcpMessages;
using System;
using System.Runtime.Remoting.Lifetime;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Server.Internal.Sessions
{
    internal abstract class DomainAgentSessionHostBase : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(DomainAgentSessionHostBase));

        private const int HandshakeTimeoutSec = 30;

        protected readonly CancellationToken Token;
        private readonly ServerAgent agent;
        private readonly ClientSponsor sponsor;
        public DomainAgentSessionClient SessionClient {get;}
        public MessageClient MessageClient {get;}

        public TaskRunnerManager Tasks {get;}
        public string AgentSessionId {get; protected set;}
        public string SessionClientId => SessionClient.Id;


        protected DomainAgentSessionHostBase(IServerSession sessionBase, ServerAgent agent, CancellationToken token)
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

            MessageClient = new MessageClient(PhotonServer.Instance.MessageRegistry);
            MessageClient.Transceiver.Context = sessionBase;

            MessageClient.ThreadException += MessageClient_OnThreadException;
        }

        public virtual void Dispose()
        {
            Tasks?.Dispose();
            MessageClient?.Dispose();
            sponsor?.Close();
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

        protected abstract Task OnBeginSession();

        protected abstract Task OnReleaseSessionAsync();

        protected abstract void OnSessionOutput(string text);

        private void SessionClient_OnSessionBegin(RemoteTaskCompletionSource taskHandle)
        {
            Task.Run(ConnectToAgent, Token)
                .ContinueWith(taskHandle.FromTask, Token);
        }

        private async Task<object> ConnectToAgent()
        {
            Log.Debug($"Connecting to TCP Agent '{agent.TcpHost}:{agent.TcpPort}'...");

            try {
                await MessageClient.ConnectAsync(agent.TcpHost, agent.TcpPort, Token);
                Log.Info($"Connected to TCP Agent '{agent.TcpHost}:{agent.TcpPort}'.");

                var handshakeRequest = new HandshakeRequest {
                    Key = Guid.NewGuid().ToString(),
                    ServerVersion = Configuration.Version,
                };

                var timeout = TimeSpan.FromSeconds(HandshakeTimeoutSec);
                var handshakeResponse = await MessageClient.Handshake<HandshakeResponse>(handshakeRequest, timeout, Token);

                if (!string.Equals(handshakeRequest.Key, handshakeResponse.Key, StringComparison.Ordinal))
                    throw new ApplicationException("Handshake Failed! An invalid key was returned.");

                if (!handshakeResponse.PasswordMatch)
                    throw new ApplicationException("Handshake Failed! Unauthorized.");
            }
            catch (Exception error) {
                Log.Error($"Failed to connect to TCP Agent '{agent.TcpHost}:{agent.TcpPort}'!", error);
                MessageClient.Dispose();
                throw;
            }

            await OnBeginSession();

            Tasks.Start();
            return null;
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
