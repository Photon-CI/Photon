using Photon.Server.Internal;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Server.ApiHandlers.Build
{
    [Secure]
    [HttpHandler("api/build/output")]
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
            var buildNumber = GetQuery<uint?>("number");

            if (string.IsNullOrEmpty(projectId))
                return Response.BadRequest().SetText("'project' is undefined!");

            if (!buildNumber.HasValue)
                return Response.BadRequest().SetText("'number' is undefined!");

            try {
                if (!PhotonServer.Instance.Projects.TryGet(projectId, out var project))
                    return Response.BadRequest().SetText($"Project '{projectId}' not found!");

                if (!project.Builds.TryGet(buildNumber.Value, out var buildData))
                    return Response.BadRequest().SetText($"Build '{buildNumber.Value}' not found!");

                var buildOutput = await buildData.GetOutput();

                return Response.Ok()
                    .SetText(buildOutput);
            }
            catch (Exception error) {
                return Response.Exception(error);
            }
        }
    }
}
