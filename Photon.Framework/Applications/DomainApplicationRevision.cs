using System;

namespace Photon.Framework.Applications
{
    [Serializable]
    public class DomainApplicationRevision
    {
        public string ProjectId {get; set;}
        public string ApplicationName {get; set;}
        public string PackageId {get; set;}
        public string PackageVersion {get; set;}
        public uint DeploymentNumber {get; set;}
        public string EnvironmentName {get; set;}
        public string ApplicationPath {get; set;}
        public DateTime CreatedTime {get; set;}
    }
}
