using Photon.Framework.Server;

namespace Photon.Framework.AgentConnection
{
    public interface IAgentConnectionClient
    {
        IAgentConnection RequestConnection(ServerAgent agent);
    }
}
