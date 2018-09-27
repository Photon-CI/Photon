using Photon.Library.Packages;
using Photon.Server.Internal;
using Photon.Server.Internal.Builds;
using Photon.Server.Internal.Security;
using PiServerLite.Http.Handlers;
using System;
using System.Collections.Generic;

namespace Photon.Server.ViewModels.Build
{
    internal class BuildDetailsVM : ServerViewModel
    {
        public string ProjectId {get; set;}
        public uint BuildNumber {get; set;}
        public string SessionId {get; private set;}
        public string ProjectName {get; private set;}
        public string BuildCreated {get; private set;}
        public string BuildDuration {get; private set;}
        public string BuildException {get; private set;}
        public string GitRefspec {get; private set;}
        public string TaskName {get; private set;}
        public string TaskRoles {get; private set;}
        public string PreBuildCommand {get; private set;}
        public string AssemblyFilename {get; private set;}
        public PackageReference[] ProjectPackages {get; private set;}
        public PackageReference[] ApplicationPackages {get; private set;}
        public List<object> Artifacts {get; private set;}
        public string IconClass {get; private set;}
        public bool IsRunning {get; private set;}
        public bool CanStartBuild {get; private set;}
        public bool CanDeleteBuild {get; private set;}


        public BuildDetailsVM(IHttpHandler handler) : base(handler) {}

        protected override void OnBuild()
        {
            base.OnBuild();

            var serverContext = PhotonServer.Instance.Context;

            CanStartBuild = !Master.IsSecured || PhotonServer.Instance.UserMgr.UserHasRole(Master.UserContext.UserId, GroupRole.BuildStart);
            CanDeleteBuild = !Master.IsSecured || PhotonServer.Instance.UserMgr.UserHasRole(Master.UserContext.UserId, GroupRole.BuildDelete);

            IconClass = "fas fa-ellipses-h text-muted";

            if (!string.IsNullOrEmpty(ProjectId) && serverContext.Projects.TryGet(ProjectId, out var project)) {
                ProjectName = project.Description.Name;

                if (project.Builds.TryGet(BuildNumber, out var buildData)) {
                    var _roles = buildData.TaskRoles != null
                        ? string.Join("; ", buildData.TaskRoles) : "<none>";

                    SessionId = buildData.ServerSessionId;
                    BuildCreated = buildData.Created.ToLocalTime().ToString("F");
                    BuildDuration = buildData.Duration?.ToString("g");
                    BuildException = buildData.Exception;
                    GitRefspec = buildData.GitRefspec;
                    TaskName = buildData.TaskName;
                    TaskRoles = _roles;
                    PreBuildCommand = buildData.PreBuildCommand;
                    AssemblyFilename = buildData.AssemblyFilename;
                    ProjectPackages = buildData.ProjectPackages;
                    ApplicationPackages = buildData.ApplicationPackages;

                    Artifacts = new List<object>();
                    // TODO

                    IconClass = GetIconClass(buildData);

                    if (!string.IsNullOrEmpty(buildData.ServerSessionId) && serverContext.Sessions.TryGet(buildData.ServerSessionId, out var session)) {
                        IsRunning = !session.IsComplete;
                    }
                }
                else {
                    Errors.Add(new ApplicationException($"Build '{BuildNumber}' not found!"));
                }
            }
            else {
                Errors.Add(new ApplicationException($"Project '{ProjectId}' not found!"));
            }
        }

        private string GetIconClass(BuildData data)
        {
            if (data.IsCancelled) return "fas fa-trash text-warning";
            if (!data.IsComplete) return "fas fa-spinner fa-spin text-info";
            if (data.IsSuccess) return "fas fa-check text-success";
            return "fas fa-exclamation-triangle text-danger";
        }
    }
}
