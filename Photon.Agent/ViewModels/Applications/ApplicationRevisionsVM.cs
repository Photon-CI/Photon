using Photon.Agent.Internal;
using PiServerLite.Http.Handlers;
using System;
using System.Linq;

namespace Photon.Agent.ViewModels.Applications
{
    internal class ApplicationRevisionsVM : AgentViewModel
    {
        public string ProjectId {get; set;}
        public string Name {get; set;}
        public RevisionRow[] Revisions {get; set;}


        public ApplicationRevisionsVM(IHttpHandler handler) : base(handler)
        {
            PageTitle = Name ?? "Photon Agent Application Revisions";
        }

        protected override void OnBuild()
        {
            base.OnBuild();

            var app = PhotonAgent.Instance.ApplicationMgr.GetApplication(ProjectId, Name);
            if (app == null) throw new ApplicationException($"Application '{Name}' not found under Project '{ProjectId}'!");

            Revisions = app.Revisions.OrderByDescending(x => x.DeploymentNumber)
                .Select(x => new RevisionRow {
                    DeploymentNumber = x.DeploymentNumber,
                    EnvironmentName = x.EnvironmentName,
                    Package = $"{x.PackageId}@{x.PackageVersion}",
                    Location = x.Location,
                    Time = x.Time.ToString("G"),
                    IsCurrent = x.IsCurrent,
                }).ToArray();
        }

        public class RevisionRow
        {
            public uint DeploymentNumber {get; set;}
            public string EnvironmentName {get; set;}
            public string Package {get; set;}
            public string Location {get; set;}
            public string Time {get; set;}
            public bool IsCurrent {get; set;}
        }
    }
}
