using Newtonsoft.Json.Linq;
using Photon.Framework;
using Photon.Framework.Extensions;
using Photon.Framework.Projects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Photon.Server.Internal.Projects
{
    internal class ProjectManager : IEnumerable<Project>
    {
        private readonly Dictionary<string, Project> projects;
        private string filename;


        public ProjectManager()
        {
            projects = new Dictionary<string, Project>();
        }

        public void Initialize()
        {
            this.filename = Configuration.ProjectsFile;

            JArray _projectList;
            using (var stream = File.Open(filename, FileMode.Open, FileAccess.Read)) {
                _projectList = JsonSettings.Serializer.Deserialize(stream);
            }

            foreach (var projectData in _projectList) {
                var project = LoadProject(projectData);
                projects[project.Id] = project;
            }
        }

        public bool TryGet(string projectId, out Project project)
        {
            return projects.TryGetValue(projectId, out project);
        }

        private Project LoadProject(dynamic data)
        {
            Project project = data.ToObject<Project>();

            switch (project.SourceType.ToLower()) {
                case "github":
                    project.Source = data.source.ToObject<ProjectGithubSource>();
                    break;
                case "fs":
                    project.Source = data.source?.ToObject<ProjectFileSystemSource>();
                    break;
                default:
                    throw new ApplicationException($"Unknown source type '{project.SourceType}'!");
            }

            return project;
        }

        public IEnumerator<Project> GetEnumerator() => projects.Values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => projects.Values.GetEnumerator();
    }
}
