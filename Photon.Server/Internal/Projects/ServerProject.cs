﻿using log4net;
using Photon.Framework;
using Photon.Framework.Extensions;
using Photon.Framework.Projects;
using Photon.Framework.Tools;
using Photon.Server.Internal.Builds;
using Photon.Server.Internal.Deployments;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Photon.Server.Internal.Projects
{
    internal class ServerProject
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ServerProject));

        private readonly object buildNumberLock;
        private readonly object deployNumberLock;
        private readonly TaskCompletionSource<object> loadTask;
        private volatile bool isLoaded;

        public string ContentPath {get; set;}
        public Project Description {get; set;}
        public ProjectLastBuild LastBuild {get; set;}
        public ProjectLastDeployment LastDeployment {get; set;}
        public BuildDataManager Builds {get; private set;}
        public DeploymentDataManager Deployments {get; private set;}

        public bool IsLoading => !isLoaded;
        public string ProjectFilename => Path.Combine(ContentPath, "project.json");
        public string LastBuildFilename => Path.Combine(ContentPath, "lastBuild.json");
        public string LastDeploymentFilename => Path.Combine(ContentPath, "lastDeployment.json");
        public string ContentBuildPath => Path.Combine(ContentPath, "builds");
        public string ContentDeploymentPath => Path.Combine(ContentPath, "deployments");


        public ServerProject()
        {
            buildNumberLock = new object();
            deployNumberLock = new object();
            loadTask = new TaskCompletionSource<object>();
        }

        public void InitializeNew()
        {
            try {
                Builds = new BuildDataManager(ContentBuildPath);
                Builds.Load();

                Deployments = new DeploymentDataManager(ContentDeploymentPath);
                Deployments.Load();
            }
            finally {
                CompleteLoading();
            }
        }

        public void Load()
        {
            try {
                LoadProject();
                LoadLastBuild();
                LoadLastDeployment();

                Builds = new BuildDataManager(ContentBuildPath);
                Builds.Load();

                Deployments = new DeploymentDataManager(ContentDeploymentPath);
                Deployments.Load();

                if (Description.MaxBuilds.HasValue)
                    Builds.Cleanup((int)Description.MaxBuilds.Value);
            }
            finally {
                CompleteLoading();
            }
        }

        public void SaveProject()
        {
            PathEx.CreatePath(ContentPath);

            using (var stream = File.Open(ProjectFilename, FileMode.Create, FileAccess.Write)) {
                JsonSettings.Serializer.Serialize(stream, Description);
            }

            CompleteLoading();
        }

        public void ReloadDescription()
        {
            LoadProject();
        }

        public async Task<BuildData> StartNewBuild()
        {
            if (!isLoaded) await loadTask.Task;

            uint buildNumber;
            lock (buildNumberLock) {
                if (LastBuild == null)
                    LastBuild = new ProjectLastBuild();

                LastBuild.Number++;
                LastBuild.Time = DateTime.Now;

                buildNumber = LastBuild.Number;
            }

            try {
                SaveLastBuild();
            }
            catch (Exception error) {
                Log.Error("Failed to update project lastBuild file!", error);
            }

            var newBuild = Builds.New(buildNumber);

            if (Description.MaxBuilds.HasValue) {
                try {
                    Builds.Cleanup((int)Description.MaxBuilds.Value);
                }
                catch {}
            }

            return newBuild;
        }

        public async Task<DeploymentData> StartNewDeployment()
        {
            if (!isLoaded) await loadTask.Task;

            uint deployNumber;
            lock (deployNumberLock) {
                if (LastDeployment == null)
                    LastDeployment = new ProjectLastDeployment();

                LastDeployment.Number++;
                LastDeployment.Time = DateTime.Now;

                deployNumber = LastDeployment.Number;
            }

            try {
                SaveLastDeployment();
            }
            catch (Exception error) {
                Log.Error("Failed to update project lastDeployment file!", error);
            }

            return Deployments.New(deployNumber);
        }

        private void LoadProject()
        {
            Description = null;
            if (!File.Exists(ProjectFilename)) return;

            using (var stream = File.Open(ProjectFilename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
                Description = JsonSettings.Serializer.Deserialize<Project>(stream);
            }
        }

        private void LoadLastBuild()
        {
            LastBuild = null;
            if (!File.Exists(LastBuildFilename)) return;

            using (var stream = File.Open(LastBuildFilename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
                LastBuild = JsonSettings.Serializer.Deserialize<ProjectLastBuild>(stream);
            }
        }

        private void LoadLastDeployment()
        {
            LastDeployment = null;
            if (!File.Exists(LastDeploymentFilename)) return;

            using (var stream = File.Open(LastDeploymentFilename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
                LastDeployment = JsonSettings.Serializer.Deserialize<ProjectLastDeployment>(stream);
            }
        }

        private void SaveLastBuild()
        {
            PathEx.CreatePath(ContentPath);

            using (var stream = File.Open(LastBuildFilename, FileMode.Create, FileAccess.Write)) {
                JsonSettings.Serializer.Serialize(stream, LastBuild);
            }
        }

        private void SaveLastDeployment()
        {
            PathEx.CreatePath(ContentPath);

            using (var stream = File.Open(LastDeploymentFilename, FileMode.Create, FileAccess.Write)) {
                JsonSettings.Serializer.Serialize(stream, LastDeployment);
            }
        }

        private void CompleteLoading()
        {
            if (isLoaded) return;

            isLoaded = true;
            loadTask.SetResult(null);
        }
    }
}
