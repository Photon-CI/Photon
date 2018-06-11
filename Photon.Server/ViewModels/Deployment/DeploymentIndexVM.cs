using Photon.Server.Internal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Photon.Server.ViewModels.Deployment
{
    internal class DeploymentIndexVM : ServerViewModel
    {
        public bool IsLoading {get; private set;}
        public DeploymentRow[] Deployments {get; private set;}
        public Framework.Projects.Project[] Projects {get; private set;}


        public void Build()
        {
            IsLoading = PhotonServer.Instance.Projects.IsLoading;
            if (IsLoading) return;

            Projects = PhotonServer.Instance.Projects.All
                .Select(x => x.Description).ToArray();

            var allDeployments = new List<DeploymentRow>();

            foreach (var project in PhotonServer.Instance.Projects.All) {
                var projectName = project.Description.Name;

                foreach (var projectDeployment in project.Deployments.AllDeployments) {
                    string @class;

                    if (projectDeployment.Created == DateTime.MinValue) {
                        @class = "fas fa-ellipsis-h text-muted";
                    }
                    else {
                        @class = !projectDeployment.IsComplete ? "fas fa-spinner fa-spin text-info"
                            : projectDeployment.IsCancelled ? "fas fa-trash text-warning"
                            : projectDeployment.IsSuccess ? "fas fa-check text-success"
                            : "fas fa-exclamation-triangle text-danger";
                    }

                    var displayTime = projectDeployment.Created > DateTime.MinValue
                        ? projectDeployment.Created.ToLocalTime().ToString("MMM d, yyyy  h:mm:ss tt")
                        : "---";

                    allDeployments.Add(new DeploymentRow {
                        ProjectId = project.Description.Id,
                        ProjectName = projectName,
                        PackageId = projectDeployment.PackageId,
                        PackageVersion = projectDeployment.PackageVersion,
                        Environment = projectDeployment.EnvironmentName,
                        Number = projectDeployment.Number,
                        Created = projectDeployment.Created,
                        CreatedDisplay = displayTime,
                        Class = @class,
                    });
                }
            }

            Deployments = allDeployments.OrderByDescending(x => x.Created)
                .ThenByDescending(x => x.Number).ToArray();
        }
    }

    internal class DeploymentRow
    {
        public string ProjectId {get; set;}
        public string ProjectName {get; set;}
        public string PackageId {get; set;}
        public string PackageVersion {get; set;}
        public string Environment {get; set;}
        public uint Number {get; set;}
        public DateTime Created {get; set;}
        public string Class {get; set;}
        public string CreatedDisplay {get; set;}
    }
}
