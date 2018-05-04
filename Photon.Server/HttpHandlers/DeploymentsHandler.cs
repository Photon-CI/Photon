using Photon.Server.Internal;
using PiServerLite.Http.Handlers;

namespace Photon.Server.HttpHandlers
{
    [HttpHandler("/deployments")]
    internal class DeploymentsHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var vm = new ViewModelBase();
            vm.Build();

            return View("Deployments.html", vm);
        }
    }
}
