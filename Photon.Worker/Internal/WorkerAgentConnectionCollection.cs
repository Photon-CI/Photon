using Photon.Framework.AgentConnection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Worker.Internal
{
    internal class WorkerAgentConnectionCollection : IWorkerAgentConnectionCollection, IDisposable
    {
        private readonly WorkerAgentConnection[] connections;
        private volatile bool isInitialized;
        private volatile bool isReleased;


        public WorkerAgentConnectionCollection(IEnumerable<WorkerAgentConnection> connections)
        {
            this.connections = connections.ToArray();
        }

        public void Dispose()
        {
            foreach (var connection in connections)
                connection.Dispose();
        }

        public async Task BeginAsync(CancellationToken token)
        {
            if (isInitialized) throw new ApplicationException("Collection has already been initialized!");
            isInitialized = true;

            var taskList = connections.Select(x => 
                Task.Run(() => x.BeginAsync(token), token)).ToArray();

            await Task.WhenAll(taskList);
        }

        public async Task ReleaseAsync(CancellationToken token)
        {
            if (isReleased) return;
            isReleased = true;

            var taskList = connections.Select(x => 
                Task.Run(() => x.ReleaseAsync(token), token)).ToArray();

            await Task.WhenAll(taskList);
        }

        public async Task RunTasksAsync(string[] taskNames, CancellationToken token)
        {
            if (!isInitialized) throw new ApplicationException("Collection has not been initialized!");
            if (isReleased) throw new ApplicationException("Collection has already been released!");

            var taskList = new List<Task>();
            foreach (var task in taskNames) {
                foreach (var connection in connections) {
                    taskList.Add(Task.Run(async () => {
                        await connection.RunTaskAsync(task, token);
                    }, token));
                }
            }

            try {
                await Task.WhenAll(taskList.ToArray());
            }
            catch (Exception error) {
                throw new Exception($"Failed to run tasks '{string.Join(";", taskNames)}'!", error);
            }
        }
    }
}
