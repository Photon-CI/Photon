using System;

namespace Photon.Framework.Applications
{
    public class DomainApplicationRevision
    {
        private readonly ApplicationRevision revision;

        public string PackageId => revision.PackageId;
        public string PackageVersion => revision.PackageVersion;
        public uint DeploymentNumber => revision.DeploymentNumber;
        public string Location => revision.Location;
        public DateTime Time => revision.Time;


        public DomainApplicationRevision(ApplicationRevision revision)
        {
            this.revision = revision;
        }
    }
}
