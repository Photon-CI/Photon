using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Photon.Server.Internal;
using PiServerLite.Http.Handlers;

namespace Photon.Server.ApiHandlers
{
    [HttpHandler("/api/configuration")]
    internal class ConfigurationHandler : HttpHandlerAsync
    {
        public override async Task<HttpHandlerResult> GetAsync(CancellationToken token)
        {
            return await Task.FromResult(Response.File(Configuration.ServerFile));
        }

        public override async Task<HttpHandlerResult> PostAsync(CancellationToken token)
        {
            using (var fileStream = File.Open(Configuration.ServerFile, FileMode.Create, FileAccess.Write)) {
                await HttpContext.Request.InputStream.CopyToAsync(fileStream);
            }

            PhotonServer.Instance.Agents.Load();

            return Response.Ok();
        }
    }
}
