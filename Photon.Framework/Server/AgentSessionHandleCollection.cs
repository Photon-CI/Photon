using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Photon.Framework.Server
{
    [Serializable]
    public class AgentSessionHandleCollection
    {
        private readonly IServerContext context;
        private readonly IAgentSessionHandle[] agentSessionList;

        public string ProjectPackageId;
        public string ProjectPackageVersion;


        public AgentSessionHandleCollection(IServerContext context, IEnumerable<IAgentSessionHandle> agentSessions)
        {
            this.context = context;

            this.agentSessionList = agentSessions as IAgentSessionHandle[] ?? agentSessions?.ToArray()
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

            try {
                await Task.WhenAll(taskList.ToArray());
            }
            catch (AggregateException errors) {
                context.Output.AppendLine("Failed to run tasks!", ConsoleColor.Red);
                errors.Flatten().Handle(e => {
                    context.Output.AppendLine($"  {e.Message}", ConsoleColor.DarkYellow);
                    return true;
                });

                throw;
            }
            catch (Exception error) {
                context.Output
                    .Append("Failed to run tasks! ", ConsoleColor.Red)
                    .AppendLine(error.Message, ConsoleColor.DarkYellow);

                throw;
            }
        }
    }
}
