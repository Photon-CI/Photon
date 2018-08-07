using Photon.Agent.ViewModels.Applications;
using PiServerLite.Http.Handlers;

namespace Photon.Agent.ViewHandlers.Applications
{
    [HttpHandler("/applications")]
    [HttpHandler("/application/index")]
    internal class ApplicationIndexHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var vm = new ApplicationIndexVM();

            return Response.View("Applications\\Index.html", vm);
        }
    }
}
