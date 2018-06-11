using Photon.Server.Internal;
using PiServerLite.Http.Handlers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Server.ApiHandlers.Deploy
{
    [HttpHandler("api/deployment/output")]
    internal class OutputStreamHandler : HttpHandlerAsync
    {
        public override async Task<HttpHandlerResult> GetAsync(CancellationToken token)
        {
            var sessionId = GetQuery<string>("session");

            if (!string.IsNullOrEmpty(sessionId)) {
                if (PhotonServer.Instance.Sessions.TryGet(sessionId, out var session)) {
                    if (!session.IsComplete)
                        return Response.Redirect("api/session/output-stream", new {id = sessionId});
                }
            }

            var projectId = GetQuery<string>("project");
            var deploymentNumber = GetQuery<uint?>("number");

            if (string.IsNullOrEmpty(projectId))
                return Response.BadRequest().SetText("'project' is undefined!");

            if (!deploymentNumber.HasValue)
                return Response.BadRequest().SetText("'number' is undefined!");

            try {
                if (!PhotonServer.Instance.Projects.TryGet(projectId, out var project))
                    return Response.BadRequest().SetText($"Project '{projectId}' not found!");

                if (!project.Deployments.TryGet(deploymentNumber.Value, out var deploymentData))
                    return Response.BadRequest().SetText($"Deployment '{deploymentNumber.Value}' not found!");

                var deploymentOutput = await deploymentData.GetOutput();

                return Response.Ok()
                    .SetText(deploymentOutput);
            }
            catch (Exception error) {
                return Response.Exception(error);
            }
        }
    }
}
