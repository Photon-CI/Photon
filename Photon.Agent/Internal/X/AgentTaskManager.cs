using Photon.Library;
using Photon.Library.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Photon.Agent.Internal
{
    internal class AgentTaskManager : IDisposable
    {
        private readonly TaskPool<AgentTask> taskPool;

        public IEnumerable<AgentTask> Tasks => taskPool.Tasks;


        public AgentTaskManager()
        {
            taskPool = new TaskPool<AgentTask>();
        }

        public void Initialize()
        {
            taskPool.Start();
        }

        public void Dispose()
        {
            taskPool?.Dispose();
        }

        public async Task<AgentStartResponse> StartTask(AgentStartRequest request)
        {
            var task = new AgentTask();
            //...

            taskPool.Add(task);

            var response = new AgentStartResponse {
                TaskId = "?",
            };

            return response;
        }
    }
}
