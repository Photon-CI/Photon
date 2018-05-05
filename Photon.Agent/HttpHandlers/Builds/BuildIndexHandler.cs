using Photon.Library;
using PiServerLite.Http.Handlers;

namespace Photon.Agent.HttpHandlers.Builds
{
    [HttpHandler("/builds")]
    [HttpHandler("/build/index")]
    internal class BuildIndexHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var vm = new ViewModelBase();
            vm.Build();

            return View("Builds\\Index.html", vm);
        }
    }
}
