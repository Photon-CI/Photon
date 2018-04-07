using Photon.Library;
using System;

namespace Photon.Server.Internal.Tasks
{
    internal class ServerTaskRunnerManager : IDisposable
    {
        private readonly ReferencePool<TaskRunner> pool;


        public ServerTaskRunnerManager()
        {
            pool = new ReferencePool<TaskRunner> {
                PruneInterval = 60_000,
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

        public void Add(TaskRunner runner)
        {
            pool.Add(runner);
        }

        public bool TryGet(string taskId, out TaskRunner taskRunner)
        {
            return pool.TryGet(taskId, out taskRunner);
        }
    }
}
