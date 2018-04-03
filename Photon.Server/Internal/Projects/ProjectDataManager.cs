using log4net;
using System.Collections.Generic;
using System.IO;

namespace Photon.Server.Internal.Projects
{
    internal class ProjectDataManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ProjectDataManager));

        private readonly Dictionary<string, ProjectData> data;
        private string projectDataPath;


        public ProjectDataManager()
        {
            data = new Dictionary<string, ProjectData>();
        }

        public void Initialize(string projectDataPath)
        {
            this.projectDataPath = projectDataPath;

            foreach (var path in Directory.EnumerateDirectories(projectDataPath, "*", SearchOption.TopDirectoryOnly)) {
                var indexFilename = Path.Combine(path, "Index.json");
                if (!File.Exists(indexFilename)) {
                    Log.Warn($"ProjectData index file not found in '{path}'.");
                    continue;
                }

                var projectData = ProjectData.Load(indexFilename);

                data[projectData.ProjectId] = projectData;
            }
        }

        public bool TryGet(string projectId, out ProjectData projectData)
        {
            return data.TryGetValue(projectId, out projectData);
        }

        public ProjectData GetOrCreate(string projectId)
        {
            if (data.TryGetValue(projectId, out var projectData))
                return projectData;

            var path = Path.Combine(projectDataPath, projectId);
            var filename = Path.Combine(path, "Index.json");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            var index = ProjectData.Create(filename, projectId);

            data[projectId] = index;

            return index;
        }
    }
}
