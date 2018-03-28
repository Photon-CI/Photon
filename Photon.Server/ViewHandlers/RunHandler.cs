using log4net;
using Newtonsoft.Json;
using Photon.Framework.Extensions;
using Photon.Framework.Sessions;
using Photon.Server.Internal;
using PiServerLite.Http.Handlers;
using System;
using System.IO;

namespace Photon.Server.Handlers
{
    [HttpHandler("/run")]
    internal class RunHandler : HttpHandler
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RunHandler));


        public override HttpHandlerResult Post()
        {
            var projectId = GetQuery("project");
            var jobName = GetQuery("job");

            Log.Debug($"Running Job '{jobName}' from Project '{projectId}'.");

            try {
                var project = PhotonServer.Instance.FindProject(projectId);
                if (project == null) return BadRequest().SetText($"Project '{projectId}' was not found!");

                var job = project.FindJob(jobName);
                if (job == null) return BadRequest().SetText($"Job '{jobName}' was not found in Project '{projectId}'!");

                var session = new ServerBuildSession(project, job);
                PhotonServer.Instance.Sessions.BeginSession(session);
                PhotonServer.Instance.Queue.Add(session);

                var response = new SessionBeginResponse {
                    SessionId = session.Id,
                };

                var memStream = new MemoryStream();

                try {
                    new JsonSerializer().Serialize(memStream, response, true);

                    return Ok()
                        .SetContentType("application/json")
                        .SetContent(memStream);
                }
                catch {
                    memStream.Dispose();
                    throw;
                }
            }
            catch (Exception error) {
                Log.Error($"Failed to run Job '{jobName}' for Project '{projectId}'!", error);
                return Exception(error);
            }
        }
    }
}
