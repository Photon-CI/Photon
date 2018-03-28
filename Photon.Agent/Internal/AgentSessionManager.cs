using log4net;
using Photon.Library;
using System;

namespace Photon.Agent.Internal
{
    internal class AgentSessionManager : IDisposable
    {
        private static ILog Log = LogManager.GetLogger(typeof(AgentSessionManager));

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

        public void BeginSession(AgentSession session)
        {
            pool.Add(session);
            Log.Info($"Started Session '{session.Id}'.");
        }

        public bool TryGetSession(string sessionId, out AgentSession session)
        {
            return pool.TryGet(sessionId, out session);
        }

        public bool ReleaseSession(string sessionId)
        {
            if (pool.TryGet(sessionId, out var session)) {
                Log.Debug($"Releasing Session '{sessionId}'...");
                session.Release();
                Log.Info($"Session '{sessionId}' released.");
                return true;
            }

            return false;
        }
    }
}
