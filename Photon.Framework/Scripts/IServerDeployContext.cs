namespace Photon.Framework.Scripts
{
    public interface IServerDeployContext : IServerContext
    {
        string ProjectPackageId {get;}
        string ProjectPackageVersion {get;}
        string ScriptName {get;}

        AgentSessionHandleCollection RegisterAgents(params string[] roles);
    }
}
