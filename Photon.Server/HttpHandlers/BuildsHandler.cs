using Photon.Library;
using PiServerLite.Http.Handlers;

namespace Photon.Server.HttpHandlers
{
    [HttpHandler("/builds")]
    internal class BuildsHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var vm = new ViewModelBase();
            vm.Build();

            return View("Builds.html", vm);
        }
    }
}
