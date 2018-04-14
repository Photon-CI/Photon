using Photon.Framework.Domain;

namespace Photon.Framework.Agent
{
    public interface IAgentContext : IDomainContext
    {
        string AgentSessionId {get;}
    }
}
