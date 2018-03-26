using Photon.Library;
using Photon.Library.Models;
using System;

namespace Photon.Agent.Internal
{
    internal class AgentSessionManager : IDisposable
    {
        private ReferencePool<AgentSession> pool;


        public AgentSessionManager()
        {
            pool = new ReferencePool<AgentSession> {
                Lifespan = 3600_000, // 60 minutes
                PruneInterval = 60_000 // 1 minute
            };
        }

        public void Dispose()
        {
            pool?.Dispose();
        }

        public void Start()
        {
            pool.Start();
        }

        public void Stop()
        {
            pool.Stop();
        }

        public AgentSession BeginSession(SessionBeginRequest request)
        {
            if (string.IsNullOrEmpty(request.ProjectName))
                throw new ApplicationException("'ProjectName' is undefined!");

            if (string.IsNullOrEmpty(request.ReleaseVersion))
                throw new ApplicationException("'ReleaseVersion' is undefined!");

            var session = new AgentSession();
            //...

            pool.Add(session);
            return session;
        }

        public bool TryGetSession(string sessionId, out AgentSession session)
        {
            return pool.TryGet(sessionId, out session);
        }

        public bool ReleaseSession(string sessionId)
        {
            if (pool.TryGet(sessionId, out var session)) {
                session.Release();
                return true;
            }

            return false;
        }
    }
}
