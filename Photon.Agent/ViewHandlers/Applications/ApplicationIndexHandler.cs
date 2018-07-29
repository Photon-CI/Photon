using Photon.Library;
using PiServerLite.Http.Handlers;

namespace Photon.Agent.ViewHandlers.Applications
{
    [HttpHandler("/applications")]
    [HttpHandler("/applications/index")]
    internal class ApplicationIndexHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var vm = new ViewModelBase();

            return Response.View("Applications\\Index.html", vm);
        }
    }
}
