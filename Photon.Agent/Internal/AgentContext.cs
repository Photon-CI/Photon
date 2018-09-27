using Photon.Agent.Internal.Session;
using System;

namespace Photon.Agent.Internal
{
    internal class AgentContext : IDisposable
    {
        public AgentSessionManager Sessions {get;}


        public AgentContext()
        {
            Sessions = new AgentSessionManager();
        }

        public void Dispose()
        {
            Sessions?.Dispose();
        }

        public void Start()
        {
            Sessions?.Start();
        }

        public void Stop()
        {
            Sessions?.Stop();
        }
    }
}
