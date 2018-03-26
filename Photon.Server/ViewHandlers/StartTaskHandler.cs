using log4net;
using Newtonsoft.Json;
using Photon.Library.Models;
using PiServerLite.Http.Handlers;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Photon.Server.Handlers
{
    [HttpHandler("/start-task")]
    internal class StartTaskHandler : HttpHandlerAsync
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(StartTaskHandler));


        public override async Task<HttpHandlerResult> PostAsync()
        {
            var projectName = GetQuery("project");

            if (string.IsNullOrWhiteSpace(projectName))
                return BadRequest().SetText("'project' is undefined!");

            var taskName = GetQuery("task");

            if (string.IsNullOrWhiteSpace(taskName))
                return BadRequest().SetText("'task' is undefined!");

            try {
                var agentResponse = await Program.Tasks.StartTask(projectName, taskName);

                var serverResponse = new ServerStartResponse {
                    TaskId = agentResponse.TaskId,
                    Agents = agentResponse.Agents.Select(x => new ServerStartAgentResponse {
                        Name = x.Name,
                        TaskId = x.TaskId,
                    }).ToList(),
                };

                return Ok()
                    .SetContentType("application/json")
                    .SetContent(responseStream => {
                        using (var responseWriter = new StreamWriter(responseStream))
                        using (var jsonWriter = new JsonTextWriter(responseWriter)) {
                            var serializer = new JsonSerializer();
                            serializer.Serialize(jsonWriter, serverResponse);
                        }
                    });
            }
            catch (Exception error) {
                Log.Error($"Failed to start Project '{projectName}' task '{taskName}'!", error);
                return Exception(error);
            }
        }
    }
}
