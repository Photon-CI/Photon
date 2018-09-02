using Photon.Framework;
using Photon.Framework.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Photon.Agent.Internal.Applications
{
    internal class ApplicationManager
    {
        private readonly ConcurrentDictionary<string, ProjectApplicationList> applicationList;

        public IEnumerable<ProjectApplicationList> All => applicationList.Values;


        public ApplicationManager()
        {
            applicationList = new ConcurrentDictionary<string, ProjectApplicationList>(StringComparer.OrdinalIgnoreCase);
        }

        public void Initialize()
        {
            if (!File.Exists(Configuration.ApplicationsFile)) return;

            Application[] apps;
            using (var stream = File.Open(Configuration.ApplicationsFile, FileMode.Open, FileAccess.Read)) {
                apps = JsonSettings.Serializer.Deserialize<Application[]>(stream);
            }

            applicationList.Clear();

            foreach (var app in apps) {
                var projectAppList = applicationList.GetOrAdd(app.ProjectId, pid => {
                    var location = Path.Combine(Configuration.ApplicationsDirectory, app.Name);
                    return new ProjectApplicationList(pid, location);
                });

                projectAppList[app.Name] = app;
            }
        }

        public void Save()
        {
            var apps = applicationList.Values
                .SelectMany(x => x).ToArray();

            using (var stream = File.Open(Configuration.ApplicationsFile, FileMode.OpenOrCreate, FileAccess.Write)) {
                JsonSettings.Serializer.Serialize(stream, apps);
            }
        }

        public Application GetApplication(string projectId, string appName)
        {
            if (!applicationList.TryGetValue(projectId, out var projectAppList)) return null;

            if (!projectAppList.TryGetApplication(appName, out var application)) return null;

            return application;
        }

        public Application RegisterApplication(string projectId, string appName)
        {
            var projectAppList = applicationList.GetOrAdd(projectId, pid =>
                new ProjectApplicationList(pid, Configuration.ApplicationsDirectory));

            return projectAppList.RegisterApplication(appName);
        }

        public void ApplyRetentionPolicy(int maxCount)
        {
            var errorList = new List<Exception>();

            var appList = applicationList.Values.ToArray();
            foreach (var app in appList) {
                try {
                    app.ApplyRetentionPolicy(maxCount);
                }
                catch (Exception error) {
                    errorList.Add(error);
                }
            }

            if (errorList.Any()) throw new AggregateException("Errors occurred while applying application retention policy!", errorList);
        }
    }
}
