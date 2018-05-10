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
        private readonly string filename;


        public ProjectManager()
        {
            projects = new Dictionary<string, Project>();
            this.filename = Configuration.ProjectsFile;
        }

        public void Load()
        {
            Project[] _projectList;
            using (var stream = File.Open(filename, FileMode.Open, FileAccess.Read)) {
                _projectList = JsonSettings.Serializer.Deserialize<Project[]>(stream);
            }

            projects.Clear();
            foreach (var project in _projectList)
                projects[project.Id] = project;
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
