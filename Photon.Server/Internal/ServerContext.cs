using Photon.Library.Packages;
using Photon.Library.Worker;
using Photon.Server.Internal.AgentConnections;
using Photon.Server.Internal.Projects;
using Photon.Server.Internal.Sessions;
using System;

namespace Photon.Server.Internal
{
    internal class ServerContext : IDisposable
    {
        public WorkerCollection Workers {get;}
        public ServerSessionManager Sessions {get;}
        public AgentConnectionManager AgentConnections {get;}
        public ProjectPackageManager ProjectPackages {get;}
        public ApplicationPackageManager ApplicationPackages {get;}
        public ProjectManager Projects {get;}
        public SessionQueue Queue {get;}


        public ServerContext()
        {
            AgentConnections = new AgentConnectionManager();
            Sessions = new ServerSessionManager();
            Workers = new WorkerCollection();
            Projects = new ProjectManager();

            ProjectPackages = new ProjectPackageManager {
                PackageDirectory = Configuration.ProjectPackageDirectory,
            };

            ApplicationPackages = new ApplicationPackageManager {
                PackageDirectory = Configuration.ApplicationPackageDirectory,
            };

            Queue = new SessionQueue {
                MaxDegreeOfParallelism = Configuration.Parallelism,
            };
        }

        public void Dispose()
        {
            End();

            Queue?.Dispose();
            AgentConnections?.Dispose();
            Sessions?.Dispose();
            Workers?.Dispose();
        }

        public void Initialize()
        {
            Workers.WorkerFilename = "?";

            Sessions.Start();
            Queue.Start();
        }

        public void End()
        {
            Queue.Stop();
            Sessions.Stop();
        }
    }
}
