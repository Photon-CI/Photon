using Photon.Framework.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Framework.Domain
{
    [Serializable]
    public class AgentSessionHandleCollection
    {
        private readonly IServerContext context;
        private readonly DomainAgentSessionHandle[] agentSessionList;
        private volatile bool isInitialized;

        public string ProjectPackageId;
        public string ProjectPackageVersion;


        public AgentSessionHandleCollection(IServerContext context, IEnumerable<DomainAgentSessionHandle> agentSessions)
        {
            this.context = context;

            this.agentSessionList = agentSessions as DomainAgentSessionHandle[] ?? agentSessions?.ToArray()
                ?? throw new ArgumentNullException(nameof(agentSessions));

            isInitialized = false;
        }

        public async Task InitializeAsync(CancellationToken token)
        {
            var taskList = agentSessionList
                .Select(x => x.BeginAsync(token))
                .ToArray();

            await Task.WhenAll(taskList);
            isInitialized = true;
        }

        public async Task ReleaseAllAsync(CancellationToken token)
        {
            var taskList = agentSessionList
                .Select(x => x.ReleaseAsync(token))
                .ToArray();

            await Task.WhenAll(taskList);
        }

        public async Task RunTasksAsync(params string[] taskNames)
        {
            if (!isInitialized) throw new ApplicationException("Agent collection has not been initialized!");

            await RunTasksAsync(taskNames, CancellationToken.None);
        }

        public async Task RunTasksAsync(string[] taskNames, CancellationToken token)
        {
            if (!isInitialized) throw new ApplicationException("Agent collection has not been initialized!");

            var taskList = new List<Task>();
            foreach (var task in taskNames) {
                foreach (var session in agentSessionList) {
                    taskList.Add(session.RunTaskAsync(task, token));
                }
            }

            try {
                await Task.WhenAll(taskList.ToArray());
            }
            catch (AggregateException errors) {
                context.Output.WriteLine("Failed to run tasks!", ConsoleColor.Red);

                errors.Flatten().Handle(e => {
                    context.Output.WriteLine($"  {e.Message}", ConsoleColor.DarkYellow);
                    return true;
                });

                throw;
            }
            catch (Exception error) {
                context.Output.Write("Failed to run tasks! ", ConsoleColor.Red)
                    .WriteLine(error.Message, ConsoleColor.DarkYellow);

                throw;
            }
        }
    }
}
