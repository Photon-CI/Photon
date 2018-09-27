using Photon.Framework.Tools;
using Photon.Library.Http;
using Photon.Server.Internal;
using Photon.Server.Internal.Security;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Server.ApiHandlers.Project
{
    [Secure]
    [RequiresRoles(GroupRole.ProjectView)]
    [HttpHandler("/api/project/json")]
    internal class JsonHandler : HttpApiHandlerAsync
    {
        public override async Task<HttpHandlerResult> GetAsync(CancellationToken token)
        {
            var serverContext = PhotonServer.Instance.Context;

            var projectId = GetQuery("id");

            if (string.IsNullOrEmpty(projectId))
                return Response.BadRequest().SetText("'id' is undefined!");

            if (!serverContext.Projects.TryGet(projectId, out var project))
                return Response.BadRequest().SetText($"Project '{projectId}' not found!");

            if (!File.Exists(project.ProjectFilename))
                return Response.BadRequest().SetText("'project.json' file could not be found!");

            return await Response.File(project.ProjectFilename)
                .SetHeader("Content-Disposition", $"attachment; filename={projectId}.json")
                .AsAsync();
        }

        // TODO: Require ProjectEdit attribute
        public override async Task<HttpHandlerResult> PostAsync(CancellationToken token)
        {
            var serverContext = PhotonServer.Instance.Context;

            var projectId = GetQuery("id");

            if (string.IsNullOrEmpty(projectId))
                return Response.BadRequest().SetText("'id' is undefined!");

            if (!serverContext.Projects.TryGet(projectId, out var project))
                return Response.BadRequest().SetText($"Project '{projectId}' not found!");

            PathEx.CreatePath(project.ContentPath);

            using (var fileStream = File.Open(project.ProjectFilename, FileMode.Create, FileAccess.Write)) {
                await HttpContext.Request.InputStream.CopyToAsync(fileStream);
            }

            project.ReloadDescription();

            return Response.Ok();
        }
    }
}
