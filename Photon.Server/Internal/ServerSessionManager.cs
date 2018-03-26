using Photon.Library;
using Photon.Library.Models;
using System;

namespace Photon.Server.Internal
{
    internal class ServerSessionManager : IDisposable
    {
        private ReferencePool<ServerSession> pool;


        public ServerSessionManager()
        {
            pool = new ReferencePool<ServerSession> {
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

        public ServerSession BeginSession(SessionBeginRequest request)
        {
            if (string.IsNullOrEmpty(request.ProjectName))
                throw new ApplicationException("'ProjectName' is undefined!");

            if (string.IsNullOrEmpty(request.ReleaseVersion))
                throw new ApplicationException("'ReleaseVersion' is undefined!");

            var session = new ServerSession {
                Request = request,
            };

            pool.Add(session);
            return session;
        }

        public bool TryGetSession(string sessionId, out ServerSession session)
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
