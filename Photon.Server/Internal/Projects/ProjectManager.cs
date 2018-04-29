using Photon.Framework;
using Photon.Framework.Extensions;
using Photon.Framework.Projects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;

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
                var project = projectData.ToObject<Project>();
                project.SourceObject = GetSource(projectData["source"], project.SourceType);

                projects[project.Id] = project;
            }
        }

        public bool TryGet(string projectId, out Project project)
        {
            return projects.TryGetValue(projectId, out project);
        }

        public IEnumerator<Project> GetEnumerator() => projects.Values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => projects.Values.GetEnumerator();

        public static object GetSource(dynamic source, string type)
        {
            switch (type.ToLower()) {
                case "github":
                    return (ProjectGithubSource)source.ToObject<ProjectGithubSource>();
                case "fs":
                    return (ProjectFileSystemSource)source?.ToObject<ProjectFileSystemSource>();
                default:
                    throw new ApplicationException($"Unknown source type '{type}'!");
            }
        }
    }
}
