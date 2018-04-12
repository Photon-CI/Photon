using Photon.Framework.Pooling;
using System;
using System.Threading.Tasks;

namespace Photon.Server.Internal.Sessions
{
    internal class ServerSessionManager : IDisposable
    {
        private readonly ReferencePool<ServerSessionBase> pool;


        public ServerSessionManager()
        {
            pool = new ReferencePool<ServerSessionBase> {
                //Lifespan = 3600_000, // 60 minutes
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

        public void BeginSession(ServerSessionBase session)
        {
            pool.Add(session);
        }

        public bool TryGet(string sessionId, out ServerSessionBase session)
        {
            return pool.TryGet(sessionId, out session);
        }

        public T GetSession<T>(string sessionId)
            where T : class, IServerSession
        {
            return pool.TryGet(sessionId, out var _session)
                ? (_session as T) : null;
        }

        public async Task<bool> ReleaseSession(string sessionId)
        {
            if (pool.TryGet(sessionId, out var session)) {
                await session.ReleaseAsync();
                return true;
            }

            return false;
        }
    }
}
