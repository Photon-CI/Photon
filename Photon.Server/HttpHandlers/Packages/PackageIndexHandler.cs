using Photon.Library;
using PiServerLite.Http.Handlers;

namespace Photon.Server.HttpHandlers.Packages
{
    [HttpHandler("/packages")]
    [HttpHandler("/package/index")]
    internal class PackageIndexHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var vm = new ViewModelBase {
                PageTitle = "Photon Server Packages",
            };

            vm.Build();

            return View("Packages\\Index.html", vm);
        }
    }
}
