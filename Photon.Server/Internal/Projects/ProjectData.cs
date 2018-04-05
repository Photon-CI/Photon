using log4net;
using Newtonsoft.Json;
using Photon.Framework.Extensions;
using System;
using System.IO;

namespace Photon.Server.Internal.Projects
{
    internal class ProjectData
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ProjectData));

        private readonly object buildNumberLock;
        private int lastBuildNumber;

        [JsonIgnore]
        public string DataPath {get; private set;}

        [JsonIgnore]
        public string Filename {get; private set;}

        [JsonProperty("projectId")]
        public string ProjectId {get; set;}

        [JsonProperty("lastBuild")]
        public ProjectDataLastBuild LastBuild {get; set;}


        public ProjectData()
        {
            buildNumberLock = new object();
        }

        public int StartNewBuild()
        {
            int buildNumber;
            lock (buildNumberLock) {
                if (LastBuild == null)
                    LastBuild = new ProjectDataLastBuild();

                LastBuild.Number++;
                LastBuild.Time = DateTime.Now;

                buildNumber = LastBuild.Number;
            }

            var path = Path.Combine(DataPath, "Builds", buildNumber.ToString());

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            try {
                Save();
            }
            catch (Exception error) {
                Log.Error("Failed to update ProjectData index file!", error);
            }

            return buildNumber;
        }

        public void Save()
        {
            using (var stream = File.Open(Filename, FileMode.Create, FileAccess.Write)) {
                var serializer = new JsonSerializer();
                serializer.Serialize(stream, this);
            }
        }

        public static ProjectData Create(string filename, string projectId)
        {
            var index = new ProjectData {
                ProjectId = projectId,
                Filename = filename,
                DataPath = Path.GetDirectoryName(filename),
            };

            index.Save();

            return index;
        }

        public static ProjectData Load(string filename)
        {
            ProjectData projectData;
            using (var stream = File.Open(filename, FileMode.Open, FileAccess.Read)) {
                var serializer = new JsonSerializer();
                projectData = serializer.Deserialize<ProjectData>(stream);
            }

            projectData.Filename = filename;
            projectData.DataPath = Path.GetDirectoryName(filename);
            projectData.lastBuildNumber = projectData.LastBuild?.Number ?? 0;

            return projectData;
        }
    }
}
