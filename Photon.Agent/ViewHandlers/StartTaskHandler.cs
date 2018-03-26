using log4net;
using Newtonsoft.Json;
using Photon.Agent.Internal;
using Photon.Library;
using Photon.Library.Models;
using PiServerLite.Http.Handlers;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Photon.Agent.Handlers
{
    [HttpHandler("/start-task")]
    internal class StartTaskHandler : HttpHandlerAsync
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(StartTaskHandler));


        public override async Task<HttpHandlerResult> PostAsync()
        {
            AgentStartRequest taskRun;
            using (var responseReader = new StreamReader(HttpContext.Request.InputStream))
            using (var jsonReader = new JsonTextReader(responseReader)) {
                var serializer = new JsonSerializer();
                taskRun = serializer.Deserialize<AgentStartRequest>(jsonReader);
            }

            var projectName = taskRun.ProjectName ?? "?";
            var taskName = taskRun.TaskName ?? "?";
            Log.Debug($"Starting Project '{projectName}' Task '{taskName}'.");

            try {
                var response = Program.Tasks.StartTask(taskRun);

                return Ok()
                    .SetContentType("application/json")
                    .SetContent(responseStream => {
                        using (var responseWriter = new StreamWriter(responseStream))
                        using (var jsonWriter = new JsonTextWriter(responseWriter)) {
                            var serializer = new JsonSerializer();
                            serializer.Serialize(jsonWriter, response);
                        }
                    });
            }
            catch (Exception error) {
                Log.Error($"Failed to start Project '{projectName}' Task '{taskName}'!", error);
                return Exception(error);
            }
        }
    }
}
