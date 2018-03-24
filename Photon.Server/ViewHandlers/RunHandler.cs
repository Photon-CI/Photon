using log4net;
using Photon.Library;
using PiServerLite.Http.Handlers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Photon.Server.Handlers
{
    [HttpHandler("/run")]
    internal class RunHandler : HttpHandlerAsync
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RunHandler));


        public override async Task<HttpHandlerResult> PostAsync()
        {
            var projectName = GetQuery("project");

            if (string.IsNullOrWhiteSpace(projectName))
                return BadRequest().SetText("'project' is undefined!");

            var taskName = GetQuery("task");

            if (string.IsNullOrWhiteSpace(taskName))
                return BadRequest().SetText("'task' is undefined!");

            try {
                await Run(projectName, taskName);

                return Ok().SetText($"Project '{projectName}' Task '{taskName}' started successfully.");
            }
            catch (Exception error) {
                Log.Error($"Failed to start Project '{projectName}' task '{taskName}'!", error);
                return Exception(error);
            }
        }

        private async Task Run(string projectName, string taskName)
        {
            var project = Program.Server?.FindProject(projectName);

            if (project == null) throw new ApplicationException($"Project '{projectName}' not found!");

            var task = project.FindTask(taskName);

            if (task == null) throw new ApplicationException($"Task '{taskName}' not found in Project '{projectName}'!");

            var agentList = Program.Server?.Definition?.Agents?.ToArray();

            if (agentList == null || !agentList.Any())
                throw new ApplicationException("No Agents are currently configured on this Server instance!");

            var projectsAgents = agentList.Where(agent => agent.MatchesRoles(task.Roles)).ToArray();

            if (!projectsAgents.Any()) {
                var roleNames = string.Join("; ", task.Roles);
                throw new ApplicationException($"No Agents were found matching roles [{roleNames}]!");
            }

            var taskList = new List<Task>();
            var errorList = new ConcurrentBag<Exception>();

            foreach (var agent in projectsAgents) {
                taskList.Add(Task.Run(async () => {
                    try {
                        await RunAgent(agent);
                    }
                    catch (Exception error) {
                        errorList.Add(error);
                    }
                }));
            }

            await Task.WhenAll(taskList.ToArray());

            if (errorList.Any()) throw new AggregateException(errorList);
        }

        private async Task RunAgent(ServerAgentDefinition agent)
        {
            //
        }
    }
}
