using System;
using Photon.Server.ViewModels.Agent;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;

namespace Photon.Server.ViewHandlers.Agent
{
    [Secure]
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
