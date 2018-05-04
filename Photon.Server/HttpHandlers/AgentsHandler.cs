using Photon.Server.ViewModels;
using PiServerLite.Http.Handlers;

namespace Photon.Server.HttpHandlers
{
    [HttpHandler("/agents")]
    internal class AgentsHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var vm = new AgentsVM();
            vm.Build();

            return View("Agents.html", vm);
        }
    }
}
