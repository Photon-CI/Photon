using Photon.Framework.Applications;
using Photon.Framework.Domain;
using Photon.Framework.Server;

namespace Photon.Framework.Agent
{
    public interface IAgentContext : IDomainContext
    {
        string AgentSessionId {get;}
        ServerAgent Agent {get;}
        IDomainApplicationClient Applications {get;}
    }
}
