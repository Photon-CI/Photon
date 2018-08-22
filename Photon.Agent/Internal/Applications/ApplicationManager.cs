using Photon.Framework;
using Photon.Framework.Extensions;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Photon.Agent.Internal.Applications
{
    internal class ApplicationManager
    {
        private const string FileName = "Applications.json";

        private readonly ConcurrentDictionary<string, ProjectApplicationList> applicationList;
        private string _filename;


        public ApplicationManager()
        {
            applicationList = new ConcurrentDictionary<string, ProjectApplicationList>(StringComparer.OrdinalIgnoreCase);
        }

        public void Initialize()
        {
            _filename = Path.Combine(Configuration.Directory, FileName);

            if (!File.Exists(_filename)) return;

            Application[] apps;
            using (var stream = File.Open(_filename, FileMode.Open, FileAccess.Read)) {
                apps = JsonSettings.Serializer.Deserialize(stream);
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

            using (var stream = File.Open(_filename, FileMode.OpenOrCreate, FileAccess.Write)) {
                JsonSettings.Serializer.Serialize(stream, apps);
            }
        }

        public Application GetApplication(string projectId, string appName)
        {
            if (!applicationList.TryGetValue(projectId, out var projectAppList))
                throw new ApplicationException($"Project '{projectId}' not found!");

            if (!projectAppList.TryGetApplication(appName, out var application))
                throw new ApplicationException($"Application '{appName}' not found!");

            return application;
        }

        public Application RegisterApplication(string projectId, string appName)
        {
            var projectAppList = applicationList.GetOrAdd(projectId, pid => {
                var location = Path.Combine(Configuration.ApplicationsDirectory, appName);
                return new ProjectApplicationList(pid, location);
            });
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

    internal class ProjectApplicationList : IEnumerable<Application>
    {
        private readonly ConcurrentDictionary<string, Application> applicationList;

        public string ProjectId {get;}
        public string AppLocation {get;}

        public Application this[string appName] {
            get => applicationList[appName];
            set => applicationList[appName] = value;
        }


        public ProjectApplicationList(string projectId, string location)
        {
            ProjectId = projectId;
            AppLocation = location;

            applicationList = new ConcurrentDictionary<string, Application>(StringComparer.OrdinalIgnoreCase);
        }

        public bool TryGetApplication(string appName, out Application application)
        {
            return applicationList.TryGetValue(appName, out application);
        }

        public Application RegisterApplication(string appName)
        {
            var app = new Application {
                ProjectId = ProjectId,
                Name = appName,
                Location = AppLocation,
            };

            applicationList[app.Name] = app;
            return app;
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

        public IEnumerator<Application> GetEnumerator()
        {
            return applicationList.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
