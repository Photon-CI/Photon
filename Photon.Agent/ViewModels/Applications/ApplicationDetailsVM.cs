using Photon.Agent.Internal;
using PiServerLite.Http.Handlers;
using System;

namespace Photon.Agent.ViewModels.Applications
{
    internal class ApplicationDetailsVM : AgentViewModel
    {
        public string ProjectId {get; set;}
        public string Name {get; set;}
        public uint DeploymentNumber {get; set;}
        public string RevisionEnvironmentName {get; set;}
        public string RevisionPackage {get; set;}
        public string RevisionLocation {get; set;}
        public string RevisionTime {get; set;}
        public bool RevisionIsCurrent {get; set;}


        public ApplicationDetailsVM(IHttpHandler handler) : base(handler)
        {
            PageTitle = Name ?? "Photon Agent Application Details";
        }

        protected override void OnBuild()
        {
            base.OnBuild();

            var app = PhotonAgent.Instance.ApplicationMgr.GetApplication(ProjectId, Name);
            if (app == null) throw new ApplicationException($"Application '{Name}' not found under Project '{ProjectId}'!");

            var revision = app.GetRevision(DeploymentNumber);
            if (revision == null) throw new ApplicationException($"Revision '{DeploymentNumber}' not found under application '{Name}', Project '{ProjectId}'!");

            RevisionEnvironmentName = revision.EnvironmentName;
            RevisionPackage = $"{revision.PackageId} @{revision.PackageVersion}";
            RevisionLocation = revision.Location;
            RevisionTime = revision.Time.ToString("F");
            RevisionIsCurrent = revision.IsCurrent;
        }
    }
}
