using Photon.Library;
using Photon.Server.Internal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Photon.Server.ViewModels.Build
{
    internal class BuildIndexVM : ViewModelBase
    {
        public BuildRow[] Builds {get; set;}


        public void Build()
        {
            var allBuilds = new List<BuildRow>();

            foreach (var projectData in PhotonServer.Instance.ProjectData.AllData) {
                var projectName = "<unknown>";

                if (PhotonServer.Instance.Projects.TryGet(projectData.ProjectId, out var project))
                    projectName = project.Name;

                allBuilds.AddRange(projectData.Builds.AllBuilds.Select(projectBuild => new BuildRow {
                    ProjectId = projectData.ProjectId,
                    ProjectName = projectName,
                    Number = projectBuild.Number,
                    Created = projectBuild.Created,
                }));
            }

            Builds = allBuilds.OrderByDescending(x => x.Created).ToArray();
        }
    }

    internal class BuildRow
    {
        public string ProjectId {get; set;}
        public string ProjectName {get; set;}
        public uint Number {get; set;}
        public DateTime Created {get; set;}
    }
}
