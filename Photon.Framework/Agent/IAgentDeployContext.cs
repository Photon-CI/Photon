namespace Photon.Framework.Agent
{
    public interface IAgentDeployContext : IAgentContext
    {
        string ProjectPackageId {get;}
        string ProjectPackageVersion {get;}
        string TaskName {get;}

        string GetApplicationDirectory(string applicationName, string applicationVersion);
    }
}
