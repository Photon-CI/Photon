using Photon.Framework;
using Photon.Framework.Extensions;
using Photon.Framework.Projects;
using System.Collections.Generic;
using System.IO;

namespace Photon.Server.Internal.Projects
{
    internal class ProjectManager
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

            Project[] _projects;
            using (var stream = File.Open(filename, FileMode.Open, FileAccess.Read)) {
                _projects = JsonSettings.Serializer.Deserialize<Project[]>(stream);
            }

            foreach (var project in _projects)
                projects[project.Id] = project;
        }

        public bool TryGet(string projectId, out Project project)
        {
            return projects.TryGetValue(projectId, out project);
        }
    }
}
