using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Server.Internal.AgentConnections
{
    internal class ServerConnectionCollection : IDisposable
    {
        private readonly AgentConnection[] connections;
        private volatile bool isInitialized;


        public ServerConnectionCollection(IEnumerable<AgentConnection> connections)
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
            var taskList = connections.Select(x => 
                Task.Run(() => x.BeginAsync(token), token)).ToArray();

            await Task.WhenAll(taskList);
            isInitialized = true;
        }

        public async Task ReleaseAsync(CancellationToken token)
        {
            var taskList = connections.Select(x => 
                Task.Run(() => x.ReleaseAsync(token), token)).ToArray();

            await Task.WhenAll(taskList);
        }

        public async Task RunTasksAsync(string[] taskNames, CancellationToken token)
        {
            if (!isInitialized) throw new ApplicationException("Agent collection has not been initialized!");

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
