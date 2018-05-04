using Photon.Server.Internal;
using PiServerLite.Http.Handlers;

namespace Photon.Server.HttpHandlers
{
    [HttpHandler("/packages")]
    internal class PackagesHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var vm = new ViewModelBase();
            vm.Build();

            return View("Packages.html", vm);
        }
    }
}
