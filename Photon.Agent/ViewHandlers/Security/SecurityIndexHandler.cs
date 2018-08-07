using Photon.Agent.ViewModels.Security;
using PiServerLite.Http.Handlers;

namespace Photon.Agent.ViewHandlers.Security
{
    [HttpHandler("/security")]
    [HttpHandler("/security/index")]
    internal class SecurityIndexHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var vm = new SecurityIndexVM();

            return Response.View("Security\\Index.html", vm);
        }
    }
}
