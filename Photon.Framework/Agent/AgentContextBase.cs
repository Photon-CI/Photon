using Photon.Framework.Domain;
using Photon.Framework.Server;

namespace Photon.Framework.Agent
{
    public abstract class AgentContextBase : DomainContextBase, IAgentContext
    {
        public string AgentSessionId {get; set;}
        public ServerAgent Agent {get; set;}
    }
}
