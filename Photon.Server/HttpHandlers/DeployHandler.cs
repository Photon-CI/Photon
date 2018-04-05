using System;
using log4net;
using Photon.Server.Internal;
using PiServerLite.Http.Handlers;

namespace Photon.Server.HttpHandlers
{
    [HttpHandler("/deploy")]
    internal class DeployHandler : HttpHandler
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(DeployHandler));


        public override HttpHandlerResult Post()
        {
            var projectId = GetQuery("project");
            var projectVersion = GetQuery("version");
            var scriptName = GetQuery("script");

            if (string.IsNullOrWhiteSpace(projectId))
                return BadRequest().SetText("'project' is undefined!");

            if (string.IsNullOrWhiteSpace(projectVersion))
                return BadRequest().SetText("'version' is undefined!");

            if (string.IsNullOrWhiteSpace(scriptName))
                return BadRequest().SetText("'script' is undefined!");

            Log.Debug($"Beginning deployment script '{scriptName}' from Project '{projectId}' @ '{projectVersion}'.");

            try {
                if (!PhotonServer.Instance.Projects.TryGet(projectId, out var project))
                    return BadRequest().SetText($"Project '{projectId}' was not found!");

                throw new NotImplementedException();

                //var job = project.FindJob(jobName);
                //if (job == null) return BadRequest().SetText($"Job '{jobName}' was not found in Project '{projectId}'!");

                //var session = new ServerDeploySession(project, job, releaseVersion);

                //PhotonServer.Instance.Sessions.BeginSession(session);
                //PhotonServer.Instance.Queue.Add(session);

                //var response = new SessionBeginResponse {
                //    SessionId = session.Id,
                //};

                //return Ok()
                //    .SetContentType("application/json")
                //    .SetContent(s => new JsonSerializer().Serialize(s, response));
            }
            catch (Exception error) {
                Log.Error($"Deployment script '{scriptName}' from Project '{projectId}' @ '{projectVersion}' has failed!", error);
                return Exception(error);
            }
        }
    }
}
