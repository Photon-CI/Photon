using Photon.Framework.Packages;
using Photon.Framework.Projects;
using System;

namespace Photon.Framework.Server
{
    public interface IServerContext : IDisposable
    {
        ServerAgentDefinition[] Agents {get;}
        ProjectPackageManager ProjectPackages {get;}
        ApplicationPackageManager ApplicationPackages {get;}

        string ServerSessionId {get;}
        Project Project {get; set;}
        string AssemblyFilename {get;}
        string WorkDirectory {get;}
        string BinDirectory {get;}
        string ContentDirectory {get;}
        ScriptOutput Output {get;}

        IAgentSessionHandle GetAgentSession(string sessionId);
        IAgentSessionHandle RegisterAnyAgent(params string[] roles);
        AgentSessionHandleCollection RegisterAllAgents(params string[] roles);
    }
}
