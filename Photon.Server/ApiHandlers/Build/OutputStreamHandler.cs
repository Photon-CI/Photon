using System;
using System.Threading;
using System.Threading.Tasks;
using Photon.Server.Internal;
using PiServerLite.Http.Handlers;

namespace Photon.Server.ApiHandlers.Build
{
    [HttpHandler("api/build/output-stream")]
    internal class OutputStreamHandler : HttpHandlerAsync
    {
        public override async Task<HttpHandlerResult> GetAsync(CancellationToken token)
        {
            var projectId = GetQuery<string>("project");
            var buildNumber = GetQuery<uint?>("number");

            if (string.IsNullOrEmpty(projectId))
                return Response.BadRequest().SetText("'project' is undefined!");

            if (!buildNumber.HasValue)
                return Response.BadRequest().SetText("'number' is undefined!");

            try {
                if (!PhotonServer.Instance.ProjectData.TryGet(projectId, out var projectData))
                    return Response.BadRequest().SetText($"Project '{projectId}' not found!");

                if (!projectData.Builds.TryGet(buildNumber.Value, out var buildData))
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
