using Photon.Library.Models;
using System;

namespace Photon.Server.Internal
{
    internal class ServerDeploySession : IServerSession
    {
        private bool isReleased;
        private DateTime utcCreated;
        private ServerDomain domain;

        public string Id {get;}
        public TimeSpan MaxLifespan {get; set;}
        public Exception Exception {get; set;}
        public SessionBeginRequest Request {get; set;}


        public ServerDeploySession()
        {
            Id = Guid.NewGuid().ToString("N");
            utcCreated = DateTime.UtcNow;
            MaxLifespan = TimeSpan.FromMinutes(60);
        }

        public void Dispose()
        {
            Release();
        }

        public void Run()
        {
            // TODO: Define Script context
            // work directory, etc

            domain = new ServerDomain();
            //...

            // TODO: Get working directory
            var outputPath = "?";

            // Create work directory

            CopyPackage(Request.ProjectName, Request.ReleaseVersion, outputPath);

            // TODO: Parse package manifest
            var assemblyFilename = "?";

            domain.Initialize(assemblyFilename);

            //...
            domain.RunScript("?", "?");
        }

        public void Release()
        {
            domain.Dispose();
            isReleased = true;
        }

        public bool IsExpired()
        {
            if (isReleased) return true;

            var elapsed = DateTime.UtcNow - utcCreated;
            return elapsed > MaxLifespan;
        }

        private void CopyPackage(string packageName, string version, string outputDirectory)
        {
            //...
        }
    }
}
