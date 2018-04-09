namespace Photon.Framework.Scripts
{
    public interface IServerDeployContext
    {
        string ProjectPackageId {get;}
        string ProjectPackageVersion {get;}
        string AssemblyFile {get;}
        string ScriptName {get;}
        string WorkDirectory {get;}
        string BinDirectory {get;}
        string ContentDirectory {get;}
        ScriptOutput Output {get;}

        AgentSessionHandleCollection RegisterAgents(params string[] roles);
    }
}
