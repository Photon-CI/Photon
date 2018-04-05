using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Photon.Framework.Scripts
{
    public class AgentSessionHandleCollection
    {
        private readonly IAgentSessionHandle[] agentSessionList;


        public AgentSessionHandleCollection(IEnumerable<IAgentSessionHandle> agentSessions)
        {
            this.agentSessionList = agentSessions as IAgentSessionHandle[] ?? agentSessions.ToArray()
                ?? throw new ArgumentNullException(nameof(agentSessions));
        }

        public async Task InitializeAsync()
        {
            var taskList = agentSessionList
                .Select(x => x.BeginAsync())
                .ToArray();

            await Task.WhenAll(taskList);
        }

        public async Task ReleaseAllAsync()
        {
            if (agentSessionList == null) return;

            var taskList = agentSessionList
                .Select(x => x.ReleaseAsync())
                .ToArray();

            await Task.WhenAll(taskList);
        }

        public async Task RunTasksAsync(params string[] taskNames)
        {
            if (agentSessionList == null) return;

            var taskList = new List<Task>();
            foreach (var task in taskNames) {
                foreach (var session in agentSessionList) {
                    taskList.Add(session.RunTaskAsync(task));
                }
            }

            await Task.WhenAll(taskList.ToArray());
        }
    }
}
