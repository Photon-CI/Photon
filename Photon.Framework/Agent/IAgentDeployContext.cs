namespace Photon.Framework.Agent
{
    public interface IAgentDeployContext : IAgentContext
    {
        uint DeploymentNumber {get; set;}
        string ProjectPackageId {get;}
        string ProjectPackageVersion {get;}
        string EnvironmentName {get; set;}
        string TaskName {get;}
        string[] AgentRoles {get;}

        string GetApplicationDirectory(string applicationName, string applicationVersion);
    }
}
