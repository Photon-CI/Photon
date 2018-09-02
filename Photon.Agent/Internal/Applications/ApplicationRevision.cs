using Photon.Framework.Tools;
using System;

namespace Photon.Agent.Internal.Applications
{
    [Serializable]
    public class ApplicationRevision
    {
        public string PackageId {get; set;}
        public string PackageVersion {get; set;}
        public uint DeploymentNumber {get; set;}
        public string EnvironmentName {get; set;}
        public string Location {get; set;}
        public DateTime Time {get; set;}


        public void Initialize()
        {
            PathEx.CreatePath(Location);
        }

        public bool Matches(uint deploymentNumber)
        {
            return DeploymentNumber == deploymentNumber;
        }

        public void Destroy()
        {
            try {
                PathEx.DestoryDirectory(Location);
            }
            catch (Exception error) {
                throw new ApplicationException($"Failed to delete application directory '{Location}'!", error);
            }
        }
    }
}
