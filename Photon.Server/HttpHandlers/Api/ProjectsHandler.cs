using Photon.Server.Internal;
using PiServerLite.Http.Handlers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Server.HttpHandlers.Api
{
    [HttpHandler("/api/projects")]
    internal class ProjectsHandler : HttpHandlerAsync
    {
        public override async Task<HttpHandlerResult> GetAsync(CancellationToken token)
        {
            return await Task.FromResult(Response.File(Configuration.ProjectsFile));
        }

        public override async Task<HttpHandlerResult> PostAsync(CancellationToken token)
        {
            using (var fileStream = File.Open(Configuration.ProjectsFile, FileMode.Create, FileAccess.Write)) {
                await HttpContext.Request.InputStream.CopyToAsync(fileStream);
            }

            PhotonServer.Instance.Projects.Load();

            return Response.Ok();
        }
    }
}
