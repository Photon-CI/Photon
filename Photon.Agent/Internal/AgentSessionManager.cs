using log4net;
using Photon.Library;
using System;
using System.Threading.Tasks;

namespace Photon.Agent.Internal
{
    internal class AgentSessionManager : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(AgentSessionManager));

        private readonly ReferencePool<IAgentSession> pool;


        public AgentSessionManager()
        {
            pool = new ReferencePool<IAgentSession> {
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

        public void BeginSession(IAgentSession session)
        {
            pool.Add(session);
            Log.Info($"Started Session '{session.SessionId}'.");
        }

        public bool TryGetSession(string sessionId, out IAgentSession session)
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
    }
}
