using Photon.Server.ViewModels.Agents;
using PiServerLite.Http.Handlers;

namespace Photon.Server.HttpHandlers.Agents
{
    [HttpHandler("/agents")]
    [HttpHandler("/agent/index")]
    internal class AgentIndexHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var vm = new AgentIndexVM {
                PageTitle = "Photon Server Agents",
            };

            vm.Build();

            return View("Agents\\Index.html", vm);
        }
    }
}
