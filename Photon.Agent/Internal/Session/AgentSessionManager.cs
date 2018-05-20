using log4net;
using Photon.Framework.Pooling;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Photon.Agent.Internal.Session
{
    internal class AgentSessionManager : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(AgentSessionManager));

        public event EventHandler<SessionStateEventArgs> SessionChanged;
        //public event EventHandler<SessionStateEventArgs> SessionReleased;

        private readonly ReferencePool<AgentSessionBase> pool;

        public IEnumerable<AgentSessionBase> All => pool.Items;
        //public IEnumerable<AgentSessionBase> Active => pool.Items.Where(i => !i.IsReleased);


        public AgentSessionManager()
        {
            pool = new ReferencePool<AgentSessionBase> {
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
            Log.Debug("Starting session pool...");
            pool.Start();
            Log.Info("Session pool started.");
        }

        public void Stop()
        {
            Log.Debug("Stopping session pool...");
            pool.Stop();
            Log.Info("Session pool stopped.");
        }

        public void Abort()
        {
            foreach (var session in pool.Items)
                session.Abort();
        }

        public void BeginSession(AgentSessionBase session)
        {
            pool.Add(session);

            session.ReleaseEvent += Session_OnReleased;
            OnSessionChanged(session);

            Log.Info($"Started Session '{session.SessionId}'.");
        }

        public bool TryGet(string sessionId, out AgentSessionBase session)
        {
            return pool.TryGet(sessionId, out session);
        }

        public async Task<bool> ReleaseSessionAsync(string sessionId)
        {
            if (!pool.TryGet(sessionId, out var session))
                return false;

            Log.Debug($"Releasing Session '{sessionId}'...");
            await session.ReleaseAsync();
            Log.Info($"Session '{sessionId}' released.");
            return true;
        }

        protected void OnSessionChanged(AgentSessionBase session)
        {
            SessionChanged?.Invoke(this, new SessionStateEventArgs(session));
        }

        private void Session_OnReleased(object sender, EventArgs e)
        {
            var session = (AgentSessionBase)sender;

            OnSessionChanged(session);
        }
    }
}
