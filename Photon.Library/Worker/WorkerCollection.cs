using System;
using System.Collections.Concurrent;

namespace Photon.Library.Worker
{
    public class WorkerCollection : IDisposable
    {
        private readonly ConcurrentDictionary<string, Worker> workerList;

        public string WorkerFilename {get; set;}


        public WorkerCollection()
        {
            WorkerFilename = "PhotonWorker.exe";

            workerList = new ConcurrentDictionary<string, Worker>();
        }

        public void Dispose()
        {
            foreach (var worker in workerList.Values)
                worker.Dispose();

            workerList.Clear();
        }

        public Worker Create(object context = null)
        {
            var worker = new Worker {
                Filename = WorkerFilename,
                ShowConsole = true,
            };

            try {
                worker.Connect();

                worker.Transceiver.Context = context;
                workerList.AddOrUpdate(worker.Id, k => worker, (k, w) => worker);

                return worker;
            }
            catch {
                worker.Dispose();
                throw;
            }
        }
    }
}
