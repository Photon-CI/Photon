using Photon.Framework.Sessions;
using Photon.Library;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Agent.Internal
{
    internal class AgentSession : IReferenceItem, IDisposable
    {
        private bool isReleased;
        private DateTime utcCreated;
        private Task initializationTask;
        private ManualResetEventSlim initializedEvent;
        private AgentSessionDomain domain;

        public string Id {get;}
        public bool IsInitialized {get; private set;}
        public TimeSpan MaxLifespan {get; set;}


        public AgentSession()
        {
            Id = Guid.NewGuid().ToString("N");
            utcCreated = DateTime.UtcNow;
            MaxLifespan = TimeSpan.FromMinutes(60);
            initializedEvent = new ManualResetEventSlim();
        }

        public void Dispose()
        {
            domain?.Dispose();
            domain = null;

            initializedEvent?.Dispose();
            initializedEvent = null;
        }

        public void Initialize(SessionBeginRequest request)
        {
            domain = new AgentSessionDomain();
            //...

            initializationTask = Task.Run(() => {
                try {
                    // TODO: Get working directory
                    var outputPath = "?";

                    DownloadPackage(request.ProjectName, request.ReleaseVersion, outputPath);

                    // TODO: Parse package manifest
                    var assemblyFilename = "?";

                    domain.Initialize(assemblyFilename);
                }
                finally {
                    IsInitialized = true;
                    initializedEvent.Set();
                }
            }).ContinueWith(t => {
                IsInitialized = true;
            });
        }

        public void Release()
        {
            domain?.Dispose();
            isReleased = true;
        }

        public bool IsExpired()
        {
            if (isReleased) return true;

            var elapsed = DateTime.UtcNow - utcCreated;
            return elapsed > MaxLifespan;
        }

        public async Task RunTask(string taskName, string jsonData = null)
        {
            if (!IsInitialized)
                initializedEvent.Wait();

            domain.RunTask(taskName, jsonData);
        }

        private void DownloadPackage(string packageName, string version, string outputDirectory)
        {
            //...
        }
    }
}
