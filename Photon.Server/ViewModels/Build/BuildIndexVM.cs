﻿using Photon.Server.Internal;
using Photon.Server.Internal.Security;
using PiServerLite.Http.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Photon.Server.ViewModels.Build
{
    internal class BuildIndexVM : ServerViewModel
    {
        public bool IsLoading {get; private set;}
        public bool CanStartBuild {get; private set;}
        public BuildRow[] Builds {get; private set;}
        public Framework.Projects.Project[] Projects {get; private set;}


        public BuildIndexVM(IHttpHandler handler) : base(handler) {}

        protected override void OnBuild()
        {
            base.OnBuild();

            CanStartBuild = !Master.IsSecured || PhotonServer.Instance.UserMgr.UserHasRole(Master.UserContext.UserId, GroupRole.BuildStart);

            IsLoading = PhotonServer.Instance.Projects.IsLoading;
            if (IsLoading) return;

            Projects = PhotonServer.Instance.Projects.All
                .Select(x => x.Description).ToArray();

            var allBuilds = new List<BuildRow>();

            foreach (var project in PhotonServer.Instance.Projects.All) {
                var projectName = project.Description.Name;

                foreach (var projectBuild in project.Builds.AllBuilds) {
                    string @class;

                    if (projectBuild.Created == DateTime.MinValue) {
                        @class = "fas fa-ellipsis-h text-muted";
                    }
                    else {
                        @class = !projectBuild.IsComplete ? "fas fa-spinner fa-spin text-info"
                            : projectBuild.IsCancelled ? "fas fa-trash text-warning"
                            : projectBuild.IsSuccess ? "fas fa-check text-success"
                            : "fas fa-exclamation-triangle text-danger";
                    }

                    var displayTime = projectBuild.Created > DateTime.MinValue
                        ? projectBuild.Created.ToLocalTime().ToString("MMM d, yyyy  h:mm:ss tt")
                        : "---";

                    allBuilds.Add(new BuildRow {
                        ProjectId = project.Description.Id,
                        ProjectName = projectName,
                        TaskName = projectBuild.TaskName,
                        Refspec = projectBuild.GitRefspec,
                        Number = projectBuild.Number,
                        Created = projectBuild.Created,
                        CreatedDisplay = displayTime,
                        Class = @class,
                    });
                }
            }

            Builds = allBuilds.OrderByDescending(x => x.Created)
                .ThenByDescending(x => x.Number).ToArray();
        }
    }

    internal class BuildRow
    {
        public string ProjectId {get; set;}
        public string ProjectName {get; set;}
        public string TaskName {get; set;}
        public string Refspec {get; set;}
        public uint Number {get; set;}
        public DateTime Created {get; set;}
        public string Class {get; set;}
        public string CreatedDisplay {get; set;}
    }
}
