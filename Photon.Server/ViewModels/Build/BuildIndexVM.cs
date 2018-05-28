using Photon.Server.Internal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Photon.Server.ViewModels.Build
{
    internal class BuildIndexVM : ServerViewModel
    {
        public BuildRow[] Builds {get; set;}


        public void Build()
        {
            var allBuilds = new List<BuildRow>();

            foreach (var project in PhotonServer.Instance.Projects.All) {
                var projectName = project.Description.Name;

                foreach (var projectBuild in project.Builds.AllBuilds) {
                    string @class;

                    if (projectBuild.Created == DateTime.MinValue) {
                        @class = "fas fa-ellipsis-h text-muted";
                    }
                    else if (projectBuild.IsComplete) {
                        @class = projectBuild.IsSuccess
                            ? "fas fa-check text-success"
                            : "fas fa-exclamation-triangle text-danger";
                    }
                    else {
                        @class = "fas fa-spinner fa-spin text-info";
                    }

                    var displayTime = projectBuild.Created > DateTime.MinValue
                        ? projectBuild.Created.ToLocalTime().ToString("MMM d, yyyy  h:mm:ss tt")
                        : "---";

                    allBuilds.Add(new BuildRow {
                        ProjectId = project.Description.Id,
                        ProjectName = projectName,
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
        public uint Number {get; set;}
        public DateTime Created {get; set;}
        public string Class {get; set;}
        public string CreatedDisplay {get; set;}
    }
}
