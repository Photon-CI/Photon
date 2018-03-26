using Photon.Library;
using Photon.Library.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Server.Internal
{
    internal class ServerSession : IReferenceItem, IDisposable
    {
        private bool isReleased;
        private DateTime utcCreated;
        private Task initializationTask;
        private ManualResetEventSlim initializedEvent;
        private ServerDomain domain;

        public string Id {get;}
        public bool IsInitialized {get; private set;}
        public TimeSpan MaxLifespan {get; set;}
        public Exception Exception {get; set;}
        public SessionBeginRequest Request {get; set;}


        public ServerSession()
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

        public async Task InitializeAsync()
        {
            // TODO: Define Script context
            // work directory, etc

            domain = new ServerDomain();
            //...

            try {
                // TODO: Get working directory
                var outputPath = "?";

                // Create work directory

                await CopyPackageAsync(Request.ProjectName, Request.ReleaseVersion, outputPath);

                // TODO: Parse package manifest
                var assemblyFilename = "?";

                domain.Initialize(assemblyFilename);
            }
            finally {
                IsInitialized = true;
                initializedEvent.Set();
            }
        }

        public async Task RunAsync()
        {
            //...
            domain.RunScript("?", "?");
        }

        public async Task CleanupAsync()
        {
            // TODO: Cleanup Work Directory

            domain.Dispose();
        }

        public void Release()
        {
            isReleased = true;
        }

        public bool IsExpired()
        {
            if (isReleased) return true;

            var elapsed = DateTime.UtcNow - utcCreated;
            return elapsed > MaxLifespan;
        }

        //public void RunTask(string taskName, string jsonData = null)
        //{
        //    if (!IsInitialized)
        //        initializedEvent.Wait();

        //    domain.RunTask(taskName, jsonData);
        //}

        private async Task CopyPackageAsync(string packageName, string version, string outputDirectory)
        {
            //...
        }
    }
}
