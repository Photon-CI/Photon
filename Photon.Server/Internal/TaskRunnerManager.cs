using Photon.Framework.Pooling;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Server.Internal
{
    internal class TaskRunnerManager : IDisposable
    {
        private readonly ReferencePool<TaskRunner> pool;


        public TaskRunnerManager()
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

        public async Task Wait(CancellationToken token)
        {
            // TODO: Wait for pending tasks!
            //pool.Items.Where(x => x.);
            await Task.Delay(3_000, token);
        }
    }
}
