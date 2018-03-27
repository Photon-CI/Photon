using Photon.Library;
using System;

namespace Photon.Server.Internal
{
    internal class ServerSessionManager : IDisposable
    {
        private ReferencePool<IServerSession> pool;


        public ServerSessionManager()
        {
            pool = new ReferencePool<IServerSession> {
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

        public void BeginSession(IServerSession session)
        {
            pool.Add(session);
        }

        //public ServerDeploySession BeginDeploySession()
        //{
        //    //if (string.IsNullOrEmpty(request.ProjectName))
        //    //    throw new ApplicationException("'ProjectName' is undefined!");

        //    //if (string.IsNullOrEmpty(request.ReleaseVersion))
        //    //    throw new ApplicationException("'ReleaseVersion' is undefined!");

        //    var session = new ServerDeploySession {
        //        //Request = request,
        //    };

        //    pool.Add(session);
        //    return session;
        //}

        public bool TryGetSession(string sessionId, out IServerSession session)
        {
            return pool.TryGet(sessionId, out session);
        }

        public T GetSession<T>(string sessionId)
            where T : class, IServerSession
        {
            return pool.TryGet(sessionId, out var _session)
                ? (_session as T) : null;
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
