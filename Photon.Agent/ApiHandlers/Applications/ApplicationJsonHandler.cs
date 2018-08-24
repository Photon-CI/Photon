using Photon.Agent.Internal;
using Photon.Library.Http;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Agent.ApiHandlers.Applications
{
    [Secure]
    [HttpHandler("/api/applications/json")]
    internal class ApplicationJsonHandler : HttpApiHandlerAsync
    {
        public override async Task<HttpHandlerResult> GetAsync(CancellationToken token)
        {
            return await Task.FromResult(Response.File(Configuration.ApplicationsFile));
        }

        //// TODO: Require ConfigurationEdit
        //public override async Task<HttpHandlerResult> PostAsync(CancellationToken token)
        //{
        //    using (var fileStream = File.Open(Configuration.AgentFile, FileMode.Create, FileAccess.Write)) {
        //        await HttpContext.Request.InputStream.CopyToAsync(fileStream);
        //    }

        //    PhotonAgent.Instance.AgentConfiguration.Load();
        //    // TODO: Reload

        //    return Response.Ok();
        //}
    }
}
