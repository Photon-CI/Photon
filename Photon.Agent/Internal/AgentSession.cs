using Photon.Library;
using Photon.Library.Models;
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

        public string Id {get;}
        private AgentSessionDomain Domain; // {get; private set;}
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
            Domain?.Dispose();
            Domain = null;

            initializedEvent?.Dispose();
            initializedEvent = null;
        }

        public void Initialize(SessionBeginRequest request)
        {
            Domain = new AgentSessionDomain();
            //...

            initializationTask = Task.Run(() => {
                try {
                    // TODO: Get working directory
                    var outputPath = "?";

                    DownloadPackage(request.ProjectName, request.ReleaseVersion, outputPath);

                    // TODO: Parse package manifest
                    var assemblyFilename = "?";

                    Domain.Initialize(assemblyFilename);
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
            Domain.Dispose();
            isReleased = true;
        }

        public bool IsExpired()
        {
            if (isReleased) return true;

            var elapsed = DateTime.UtcNow - utcCreated;
            return elapsed > MaxLifespan;
        }

        public void RunTask(string taskName, string jsonData = null)
        {
            if (!IsInitialized)
                initializedEvent.Wait();

            Domain.RunTask(taskName, jsonData);
        }

        private void DownloadPackage(string packageName, string version, string outputDirectory)
        {
            //...
        }
    }
}
