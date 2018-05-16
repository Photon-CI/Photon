using System;
using System.IO;
using Photon.Framework.Server;

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
        public ServerAgent Agent {get; set;}


        public string GetApplicationDirectory(string applicationName, string applicationVersion)
        {
            return Path.Combine(ApplicationsDirectory, applicationName, applicationVersion);
        }
    }
}
