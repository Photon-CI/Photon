using System;
using Photon.Server.ViewModels.Agents;
using PiServerLite.Http.Handlers;

namespace Photon.Server.HttpHandlers.Agents
{
    [HttpHandler("/agent/edit")]
    internal class AgentEditHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var vm = new AgentEditVM {
                AgentId = GetQuery("id"),
            };

            vm.PageTitle = $"Photon Server {(vm.IsNew?"New":"Edit")} Agent";

            vm.Build();

            return View("Agents\\Edit.html", vm);
        }

        public override HttpHandlerResult Post()
        {
            var vm = new AgentEditVM();

            try {
                vm.Restore(As.FormData());
                vm.Save();

                return Redirect("Agents");
            }
            catch (Exception error) {
                vm.Errors.Add(error);
                return View("Agents\\Edit.html", vm);
            }
        }
    }
}
