using Newtonsoft.Json;
using Photon.Framework.Extensions;
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
            this.filename = Configuration.;

            Project[] _projects;
            using (var stream = File.Open(filename, FileMode.Open, FileAccess.Read)) {
                var serializer = new JsonSerializer();
                _projects = serializer.Deserialize<Project[]>(stream);
            }

            foreach (var project in _projects)
                projects[project.Id] = project;
        }
    }
}
