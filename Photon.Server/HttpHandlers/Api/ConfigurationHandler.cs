using Photon.Server.Internal;
using PiServerLite.Http.Handlers;
using System.IO;
using System.Threading.Tasks;

namespace Photon.Server.HttpHandlers.Api
{
    [HttpHandler("/api/configuration")]
    internal class ConfigurationHandler : HttpHandlerAsync
    {
        public override async Task<HttpHandlerResult> GetAsync()
        {
            return await Task.FromResult(Ok()
                .SetContentType("text/plain")
                .SetContent(async (responseStream, token) => {
                    using (var fileStream = File.Open(Configuration.ServerFile, FileMode.Open, FileAccess.Read)) {
                        await fileStream.CopyToAsync(responseStream);
                    }
                }));
        }

        public override async Task<HttpHandlerResult> PostAsync()
        {
            using (var fileStream = File.Open(Configuration.ServerFile, FileMode.Create, FileAccess.Write)) {
                await HttpContext.Request.InputStream.CopyToAsync(fileStream);
            }

            PhotonServer.Instance.Agents.Load();

            return Ok();
        }
    }
}
