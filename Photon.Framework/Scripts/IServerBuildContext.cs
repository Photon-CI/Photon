using Photon.Framework.Projects;

namespace Photon.Framework.Scripts
{
    public interface IServerBuildContext
    {
        Project Project {get;}
        string AssemblyFile {get;}
        string ScriptName {get;}
        string WorkDirectory {get;}
        int BuildNumber {get;}

        AgentSessionHandleCollection RegisterAgents(params string[] roles);
    }
}
