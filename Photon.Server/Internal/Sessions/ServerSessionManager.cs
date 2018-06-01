using log4net;
using Photon.Framework.Pooling;
using System;
using System.Collections.Generic;

namespace Photon.Server.Internal.Sessions
{
    internal class ServerSessionManager : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ServerSessionManager));

        public event EventHandler<SessionStateEventArgs> SessionChanged;

        private readonly ReferencePool<ServerSessionBase> pool;

        public IEnumerable<ServerSessionBase> All => pool.Items;


        public ServerSessionManager()
        {
            pool = new ReferencePool<ServerSessionBase> {
                PruneInterval = 600_000, // 10 minutes
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
            OnSessionChanged(session);

            Log.Info($"Started Session '{session.SessionId}'.");
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

        protected void OnSessionChanged(ServerSessionBase session)
        {
            SessionChanged?.Invoke(this, new SessionStateEventArgs(session));
        }

        private void Session_OnReleased(object sender, EventArgs e)
        {
            var session = (ServerSessionBase)sender;

            OnSessionChanged(session);
        }
    }
}
