using Photon.Framework.Domain;
using System;

namespace Photon.Framework.Agent
{
    [Serializable]
    public abstract class AgentContextBase : DomainContextBase, IAgentContext
    {
        public string AgentSessionId {get; set;}
    }
}
