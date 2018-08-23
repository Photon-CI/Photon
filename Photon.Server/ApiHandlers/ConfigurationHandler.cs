using Photon.Library.Http;
using Photon.Server.Internal;
using Photon.Server.Internal.Security;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Server.ApiHandlers
{
    [Secure]
    [RequiresRoles(GroupRole.ConfigurationView)]
    [HttpHandler("/api/configuration")]
    internal class ConfigurationHandler : HttpApiHandlerAsync
    {
        public override async Task<HttpHandlerResult> GetAsync(CancellationToken token)
        {
            return await Task.FromResult(Response.File(Configuration.ServerFile));
        }

        // TODO: Require ConfigurationEdit
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
