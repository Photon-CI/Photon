namespace Photon.Framework.Agent
{
    public interface IAgentDeployContext : IAgentContext
    {
        uint DeploymentNumber {get; set;}
        string ProjectPackageId {get;}
        string ProjectPackageVersion {get;}
        string TaskName {get;}

        string GetApplicationDirectory(string applicationName, string applicationVersion);
    }
}
