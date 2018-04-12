using Photon.Framework.Domain;
using System;

namespace Photon.Framework.Server
{
    public interface IServerContext : IDomainContext, IDisposable
    {
        ServerAgentDefinition[] Agents {get;}
        string ServerSessionId {get;}

        //DomainAgentSessionHandle GetAgentSession(string agentSessionId);
        DomainAgentSessionHandle RegisterAnyAgent(params string[] roles);
        AgentSessionHandleCollection RegisterAllAgents(params string[] roles);
    }
}
