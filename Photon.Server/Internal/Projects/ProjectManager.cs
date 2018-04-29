using Photon.Framework;
using Photon.Framework.Extensions;
using Photon.Framework.Projects;
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

            Project[] _projectList;
            using (var stream = File.Open(filename, FileMode.Open, FileAccess.Read)) {
                _projectList = JsonSettings.Serializer.Deserialize<Project[]>(stream);
            }

            foreach (var project in _projectList)
                projects[project.Id] = project;
        }

        public bool TryGet(string projectId, out Project project)
        {
            return projects.TryGetValue(projectId, out project);
        }

        public IEnumerator<Project> GetEnumerator() => projects.Values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => projects.Values.GetEnumerator();
    }
}
