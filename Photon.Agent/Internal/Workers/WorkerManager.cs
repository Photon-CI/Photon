using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

namespace Photon.Agent.Internal.Workers
{
    internal class WorkerManager : IDisposable
    {
        private readonly ConcurrentDictionary<string, Worker> workers;

        public string WorkerFilename {get; set;}


        public WorkerManager()
        {
            workers = new ConcurrentDictionary<string, Worker>(StringComparer.Ordinal);
        }

        public void Dispose()
        {
            var allWorkers = workers.Values.ToArray();
            workers.Clear();

            foreach (var worker in allWorkers)
                worker.Dispose();
        }

        public Worker Register(CancellationToken cancellationToken = default(CancellationToken))
        {
            var worker = new Worker {
                SessionId = Guid.NewGuid().ToString("N"),
                Filename = WorkerFilename,
            };

            var port = 0;
            while (!cancellationToken.IsCancellationRequested && !ports.Register(worker, out port)) {
                releaseEvent.WaitOne();
            }

            cancellationToken.ThrowIfCancellationRequested();

            worker.Port = port;
            //...

            workers[worker.SessionId] = worker;
            return worker;
        }

        public void Release(string sessionId)
        {
            if (workers.TryRemove(sessionId, out var worker)) {
                ports.Release(worker.Port);
                worker.Dispose();
            }
        }
    }
}
