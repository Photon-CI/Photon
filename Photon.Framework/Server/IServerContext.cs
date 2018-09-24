using Photon.Framework.Domain;
using System;

namespace Photon.Framework.Server
{
    public interface IServerContext : IDomainContext, IDisposable
    {
        ServerAgent[] Agents {get;}
        string ServerSessionId {get;}

        //DomainAgentSessionHandle GetAgentSession(string agentSessionId);
        WorkerAgentSelector RegisterAgents {get;}
    }
}
