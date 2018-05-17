using Photon.Framework.Domain;
using Photon.Framework.Server;
using System;

namespace Photon.Framework.Agent
{
    [Serializable]
    public abstract class AgentContextBase : DomainContextBase, IAgentContext
    {
        public string AgentSessionId {get; set;}
        public ServerAgent Agent {get; set;}
    }
}
