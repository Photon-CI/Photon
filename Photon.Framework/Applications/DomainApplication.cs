using System;
using System.Collections.Generic;

namespace Photon.Framework.Applications
{
    [Serializable]
    public class DomainApplication
    {
        private readonly Application application;

        public string Name => application.Name;
        public string ProjectId => application.ProjectId;
        public string Location => application.Location;
        public List<DomainApplicationRevision> Revisions {get; set;}


        public DomainApplication(Application application)
        {
            this.application = application;
        }

        public bool TryGetRevision(uint deploymentNumber, out DomainApplicationRevision revision)
        {
            var rev = application.GetRevision(deploymentNumber);
            revision = rev == null ? null : new DomainApplicationRevision(rev);
            return revision != null;
        }

        public DomainApplicationRevision RegisterRevision(ApplicationRevision revision)
        {
            if (revision == null) throw new ArgumentNullException(nameof(revision));

            var pathName = revision.DeploymentNumber.ToString();
            revision.Location = NetPath.Combine(Location, pathName);
            revision.Time = DateTime.Now;

            application.Revisions.Add(revision);
            revision.Initialize();

            return new DomainApplicationRevision(revision);
        }
    }
}
