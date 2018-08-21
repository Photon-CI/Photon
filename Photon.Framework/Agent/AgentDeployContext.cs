using System;

namespace Photon.Framework.Agent
{
    [Serializable]
    public class AgentDeployContext : AgentContextBase, IAgentDeployContext
    {
        public uint DeploymentNumber {get; set;}
        public string ProjectPackageId {get; set;}
        public string ProjectPackageVersion {get; set;}
        public string TaskName {get; set;}
        public string ApplicationsDirectory {get; set;}
        public string EnvironmentName {get; set;}
    }
}
