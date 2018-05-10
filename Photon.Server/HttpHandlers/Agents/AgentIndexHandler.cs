using Photon.Server.ViewModels.Agents;
using PiServerLite.Http.Handlers;
using System;

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

            try {
                vm.Build();
            }
            catch (Exception error) {
                vm.Errors.Add(error);
            }

            return Response.View("Agents\\Index.html", vm);
        }
    }
}
