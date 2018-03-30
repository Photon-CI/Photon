using log4net;
using Newtonsoft.Json;
using Photon.Framework.Extensions;
using Photon.Framework.Messages;
using Photon.Server.Internal;
using PiServerLite.Http.Handlers;
using System;

namespace Photon.Server.Handlers
{
    [HttpHandler("/deploy")]
    internal class DeployHandler : HttpHandler
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(DeployHandler));


        public override HttpHandlerResult Post()
        {
            var projectId = GetQuery("project");
            var jobName = GetQuery("job");
            var releaseVersion = GetQuery("release");

            if (string.IsNullOrWhiteSpace(releaseVersion))
                return BadRequest().SetText("'releaseVersion' is undefined!");

            Log.Debug($"Beginning deployment of Job '{jobName}' from Project '{projectId}' @ '{releaseVersion}'.");

            try {
                var project = PhotonServer.Instance.FindProject(projectId);
                if (project == null) return BadRequest().SetText($"Project '{projectId}' was not found!");

                var job = project.FindJob(jobName);
                if (job == null) return BadRequest().SetText($"Job '{jobName}' was not found in Project '{projectId}'!");

                var session = new ServerDeploySession(project, job, releaseVersion);

                PhotonServer.Instance.Sessions.BeginSession(session);
                PhotonServer.Instance.Queue.Add(session);

                var response = new SessionBeginResponse {
                    SessionId = session.Id,
                };

                return Ok()
                    .SetContentType("application/json")
                    .SetContent(s => new JsonSerializer().Serialize(s, response));
            }
            catch (Exception error) {
                Log.Error($"Deployment of Job '{jobName}' from Project '{projectId}' @ '{releaseVersion}'!", error);
                return Exception(error);
            }
        }
    }
}
