using log4net;
using Newtonsoft.Json;
using Photon.Framework;
using Photon.Framework.Extensions;
using Photon.Framework.Tools;
using Photon.Server.Internal.Builds;
using System;
using System.IO;

namespace Photon.Server.Internal.Projects
{
    internal class ProjectData
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ProjectData));

        private readonly object buildNumberLock;
        private readonly object deployNumberLock;

        [JsonIgnore]
        public string DataPath {get; private set;}

        [JsonIgnore]
        public string Filename {get; private set;}

        public string ProjectId {get; set;}

        public ProjectDataLastBuild LastBuild {get; set;}

        public ProjectDataLastBuild LastDeployment {get; set;}

        [JsonIgnore]
        public BuildDataManager Builds {get; private set;}


        public ProjectData()
        {
            buildNumberLock = new object();
            deployNumberLock = new object();
        }

        public void Initialize()
        {
            var buildsPath = Path.Combine(DataPath, "builds");

            Builds = new BuildDataManager(buildsPath);
            Builds.Load();
        }

        public uint StartNewBuild()
        {
            uint buildNumber;
            lock (buildNumberLock) {
                if (LastBuild == null)
                    LastBuild = new ProjectDataLastBuild();

                LastBuild.Number++;
                LastBuild.Time = DateTime.Now;

                buildNumber = LastBuild.Number;
            }

            var buildPath = Path.Combine(DataPath, "Builds", buildNumber.ToString());
            PathEx.CreatePath(buildPath);

            try {
                Save();
            }
            catch (Exception error) {
                Log.Error("Failed to update ProjectData index file!", error);
            }

            return buildNumber;
        }

        public uint StartNewDeployment()
        {
            uint deployNumber;
            lock (deployNumberLock) {
                if (LastDeployment == null)
                    LastDeployment = new ProjectDataLastBuild();

                LastDeployment.Number++;
                LastDeployment.Time = DateTime.Now;

                deployNumber = LastDeployment.Number;
            }

            var deployPath = Path.Combine(DataPath, "Deployments", deployNumber.ToString());
            PathEx.CreatePath(deployPath);

            try {
                Save();
            }
            catch (Exception error) {
                Log.Error("Failed to update ProjectData index file!", error);
            }

            return deployNumber;
        }

        public void Save()
        {
            PathEx.CreateFilePath(Filename);

            using (var stream = File.Open(Filename, FileMode.Create, FileAccess.Write)) {
                JsonSettings.Serializer.Serialize(stream, this);
            }
        }

        public static ProjectData Create(string filename, string projectId)
        {
            var index = new ProjectData {
                ProjectId = projectId,
                Filename = filename,
                DataPath = Path.GetDirectoryName(filename),
            };

            index.Initialize();
            index.Save();

            return index;
        }

        public static ProjectData Load(string filename)
        {
            ProjectData projectData;
            using (var stream = File.Open(filename, FileMode.Open, FileAccess.Read)) {
                projectData = JsonSettings.Serializer.Deserialize<ProjectData>(stream);
            }

            projectData.Filename = filename;
            projectData.DataPath = Path.GetDirectoryName(filename);
            projectData.Initialize();

            return projectData;
        }
    }
}
