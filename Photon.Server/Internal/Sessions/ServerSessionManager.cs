using Photon.Framework.Pooling;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Photon.Server.Internal.Sessions
{
    internal class ServerSessionManager : IDisposable
    {
        public event EventHandler<SessionStateEventArgs> SessionStarted;
        public event EventHandler<SessionStateEventArgs> SessionReleased;

        private readonly ReferencePool<ServerSessionBase> pool;

        public IEnumerable<ServerSessionBase> Active => pool.Items.Where(i => !i.IsReleased);


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

        public void Abort()
        {
            foreach (var session in pool.Items)
                session.Abort();
        }

        public void BeginSession(ServerSessionBase session)
        {
            pool.Add(session);

            session.ReleaseEvent += Session_OnReleased;

            OnSessionStarted(session);
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

        //public async Task<bool> ReleaseSession(string sessionId)
        //{
        //    if (pool.TryGet(sessionId, out var session)) {
        //        await session.ReleaseAsync();
        //        OnSessionReleased(session);
        //        return true;
        //    }

        //    return false;
        //}

        protected void OnSessionStarted(ServerSessionBase session)
        {
            SessionStarted?.Invoke(this, new SessionStateEventArgs(session));
        }

        protected void OnSessionReleased(ServerSessionBase session)
        {
            SessionReleased?.Invoke(this, new SessionStateEventArgs(session));
        }

        private void Session_OnReleased(object sender, EventArgs e)
        {
            var session = (ServerSessionBase)sender;

            OnSessionReleased(session);
        }
    }
}
