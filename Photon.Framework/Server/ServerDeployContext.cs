using System;

namespace Photon.Framework.Server
{
    [Serializable]
    public class ServerDeployContext : ServerContextBase, IServerDeployContext
    {
        public uint DeploymentNumber {get; set;}
        public string ProjectPackageId {get; set;}
        public string ProjectPackageVersion {get; set;}
        public string EnvironmentName {get; set;}
        public string ScriptName {get; set;}
    }
}
