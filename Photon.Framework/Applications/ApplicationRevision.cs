using Photon.Framework.Tools;
using System;

namespace Photon.Framework.Applications
{
    public class ApplicationRevision
    {
        public string PackageId {get; set;}
        public string PackageVersion {get; set;}
        public uint DeploymentNumber {get; set;}
        public string Location {get; internal set;}
        public DateTime Time {get; internal set;}


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
