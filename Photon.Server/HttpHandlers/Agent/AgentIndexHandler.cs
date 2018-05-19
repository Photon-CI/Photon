using PiServerLite.Http.Handlers;
using System;
using Photon.Server.ViewModels.Agent;

namespace Photon.Server.HttpHandlers.Agent
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

            return Response.View("Agent\\Index.html", vm);
        }
    }
}
