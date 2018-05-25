using Photon.Framework.Packages;
using Photon.Library;
using Photon.Server.Internal;
using System;

namespace Photon.Server.ViewModels.Build
{
    internal class BuildDetailsVM : ViewModelBase
    {
        public string ProjectId {get; set;}
        public string ProjectName {get; set;}
        public uint BuildNumber {get; set;}
        public string BuildCreated {get; set;}
        public string BuildDuration {get; set;}
        public PackageReference[] ProjectPackages {get; set;}
        public bool IsRunning {get; set;}


        public void Build()
        {
            if (!string.IsNullOrEmpty(ProjectId) && PhotonServer.Instance.Projects.TryGet(ProjectId, out var project)) {
                ProjectName = project.Name;
            }
            else {
                Errors.Add(new ApplicationException($"Project '{ProjectId}' not found!"));
            }

            if (!string.IsNullOrEmpty(ProjectId) && PhotonServer.Instance.ProjectData.TryGet(ProjectId, out var projectData)) {
                if (projectData.Builds.TryGet(BuildNumber, out var buildData)) {
                    BuildCreated = buildData.Created.ToLocalTime().ToString("F");
                    BuildDuration = buildData.Duration?.ToString("g");
                    ProjectPackages = buildData.ProjectPackages;

                    if (!string.IsNullOrEmpty(buildData.ServerSessionId) && PhotonServer.Instance.Sessions.TryGet(buildData.ServerSessionId, out var session)) {
                        IsRunning = !session.IsComplete;
                    }
                }
                else {
                    Errors.Add(new ApplicationException($"Build '{BuildNumber}' not found!"));
                }
            }
            else {
                Errors.Add(new ApplicationException($"Project Data '{ProjectId}' not found!"));
            }
        }
    }
}
