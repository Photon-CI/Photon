using Photon.Framework.Applications;

namespace Photon.Framework.Agent
{
    public interface IAgentDeployContext : IAgentContext
    {
        uint DeploymentNumber {get;}
        string ProjectPackageId {get;}
        string ProjectPackageVersion {get;}
        string EnvironmentName {get;}
        string TaskName {get;}
        IApplicationWriter Applications {get; set;}
    }
}
