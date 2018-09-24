using System;
using Photon.Library.Worker;
using Photon.Server.Internal.AgentConnections;
using Photon.Server.Internal.Sessions;

namespace Photon.Server.Internal.New
{
    internal class ServerContext : IDisposable
    {
        public AgentConnectionManager AgentConnections {get;}
        public ServerSessionManager Sessions {get;}
        public WorkerCollection Workers {get;}


        public ServerContext()
        {
            AgentConnections = new AgentConnectionManager();
            Sessions = new ServerSessionManager();
            Workers = new WorkerCollection();
        }

        public void Dispose()
        {
            End();

            AgentConnections?.Dispose();
            Sessions?.Dispose();
            Workers?.Dispose();
        }

        public void Initialize()
        {
            Workers.WorkerFilename = "?";

            Sessions.Start();
        }

        public void End()
        {
            Sessions.Stop();
        }
    }
}
