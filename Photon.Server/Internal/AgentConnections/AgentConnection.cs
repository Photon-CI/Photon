using log4net;
using Photon.Communication;
using Photon.Framework.Server;
using Photon.Library.Communication;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Server.Internal.AgentConnections
{
    internal class AgentConnection : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(AgentConnection));

        public event EventHandler<AgentConnectionReleaseEventArgs> Released;

        private bool isReleased;
        private MessageClient client;

        public ServerAgent Agent {get;}
        public string ConnectionId {get;}
        public MessageTransceiver Transceiver => client?.Transceiver;


        public AgentConnection(ServerAgent agent)
        {
            this.Agent = agent;

            ConnectionId = Guid.NewGuid().ToString("D");
        }

        public void Dispose()
        {
            if (!isReleased) {
                OnReleased();
                isReleased = true;
            }

            client?.Dispose();
            client = null;

            //...
        }

        public async Task BeginAsync(CancellationToken token = default)
        {
            client = new MessageClient(PhotonServer.Instance.MessageRegistry);

            try {
                await client.ConnectAsync(Agent.TcpHost, Agent.TcpPort, token);

                await ClientHandshake.Verify(client, Configuration.Version, token);

                Log.Info($"Connected to Agent '{Agent.Name}' successfully.");
            }
            catch (Exception error) {
                Log.Error($"Failed to connect to Agent '{Agent.Name}'!", error);
                throw;
            }
        }

        public void Release()
        {
            if (isReleased) return;
            isReleased = true;

            OnReleased();
        }

        public async Task RunTaskAsync(string taskName, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        protected virtual void OnReleased()
        {
            var e = new AgentConnectionReleaseEventArgs(Agent, ConnectionId);
            Released?.Invoke(this, e);
        }
    }

    internal class AgentConnectionReleaseEventArgs : EventArgs
    {
        public string ConnectionId {get; set;}
        public ServerAgent Agent {get;}


        public AgentConnectionReleaseEventArgs(ServerAgent agent, string connectionId)
        {
            this.Agent = agent;
            this.ConnectionId = connectionId;
        }
    }
}
