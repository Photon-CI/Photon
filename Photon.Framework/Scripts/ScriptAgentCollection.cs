using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Photon.Framework.Scripts
{
    public class ScriptAgentCollection
    {
        private readonly ScriptAgent[] scriptAgents;
        private ScriptAgentSession[] sessions;


        public ScriptAgentCollection(IEnumerable<ScriptAgent> scriptAgents)
        {
            if (scriptAgents == null)
                throw new ArgumentNullException(nameof(scriptAgents));

            this.scriptAgents = scriptAgents as ScriptAgent[] ?? scriptAgents.ToArray();
        }

        public async Task InitializeAsync()
        {
            var taskList = scriptAgents
                .Select(x => x.BeginSession())
                .ToArray();

            await Task.WhenAll(taskList);

            sessions = taskList
                .Select(t => t.Result)
                .ToArray();
        }

        public async Task ReleaseAsync()
        {
            if (sessions == null) return;

            var taskList = sessions
                .Select(x => x.ReleaseAsync())
                .ToArray();

            await Task.WhenAll(taskList);
        }

        public async Task RunTasks(params string[] taskNames)
        {
            if (sessions == null) return;

            var taskList = new List<Task>();
            foreach (var task in taskNames) {
                foreach (var session in sessions) {
                    taskList.Add(session.RunTask(task));
                }
            }

            await Task.WhenAll(taskList.ToArray());

            throw new NotImplementedException();
        }
    }
}
