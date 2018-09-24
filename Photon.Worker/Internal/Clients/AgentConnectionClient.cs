using Photon.Framework.Agent;
using Photon.Framework.AgentConnection;
using Photon.Framework.Server;

namespace Photon.Worker.Internal.Clients
{
    internal class AgentConnectionClient : IAgentConnectionClient
    {
        private readonly ServerSession;

        public IAgentConnection RequestConnection(ServerAgent agent)
        {
            var host = OnCreateHost(agent);
            hostList[host.SessionClientId] = host;

            return host.SessionClient;
        }
    }
}
