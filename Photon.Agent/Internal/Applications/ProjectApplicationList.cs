using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Photon.Agent.Internal.Applications
{
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
            var location = Path.Combine(AppLocation, appName);

            var app = new Application {
                ProjectId = ProjectId,
                Name = appName,
                Location = location,
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
