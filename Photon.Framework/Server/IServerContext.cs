using Photon.Framework.Domain;
using Photon.Framework.Packages;
using System;

namespace Photon.Framework.Server
{
    public interface IServerContext : IDomainContext, IDisposable
    {
        ServerAgentDefinition[] Agents {get;}
        ProjectPackageManager ProjectPackages {get;}
        ApplicationPackageManager ApplicationPackages {get;}

        string ServerSessionId {get;}

        IAgentSessionHandle GetAgentSession(string sessionId);
        IAgentSessionHandle RegisterAnyAgent(params string[] roles);
        AgentSessionHandleCollection RegisterAllAgents(params string[] roles);
    }
}
