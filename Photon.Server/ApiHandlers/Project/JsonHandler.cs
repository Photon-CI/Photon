using Photon.Framework.Tools;
using Photon.Server.Internal;
using PiServerLite.Http.Handlers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Server.ApiHandlers.Project
{
    [HttpHandler("/api/project/json")]
    internal class JsonHandler : HttpHandlerAsync
    {
        public override async Task<HttpHandlerResult> GetAsync(CancellationToken token)
        {
            var projectId = GetQuery("id");

            if (string.IsNullOrEmpty(projectId))
                return Response.BadRequest().SetText("'id' is undefined!");

            if (!PhotonServer.Instance.Projects.TryGet(projectId, out var project))
                return Response.BadRequest().SetText($"Project '{projectId}' not found!");

            if (!File.Exists(project.DescriptionFilename))
                return Response.BadRequest().SetText("'project.json' file could not be found!");

            return await Response.File(project.DescriptionFilename)
                .SetHeader("Content-Disposition", $"attachment; filename={projectId}.json")
                .AsAsync();
        }

        public override async Task<HttpHandlerResult> PostAsync(CancellationToken token)
        {
            var projectId = GetQuery("id");

            if (string.IsNullOrEmpty(projectId))
                return Response.BadRequest().SetText("'id' is undefined!");

            if (!PhotonServer.Instance.Projects.TryGet(projectId, out var project))
                return Response.BadRequest().SetText($"Project '{projectId}' not found!");

            PathEx.CreatePath(project.ContentPath);

            using (var fileStream = File.Open(project.DescriptionFilename, FileMode.Create, FileAccess.Write)) {
                await HttpContext.Request.InputStream.CopyToAsync(fileStream);
            }

            project.Load();

            return Response.Ok();
        }
    }
}
