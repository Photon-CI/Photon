using System;
using Photon.Agent.ViewModels.VariableSet;
using PiServerLite.Http.Handlers;

namespace Photon.Agent.HttpHandlers.VariableSet
{
    [HttpHandler("/variables")]
    [HttpHandler("/variable/index")]
    internal class VariableSetIndexHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var vm = new VariableSetIndexVM {
                PageTitle = "Photon Agent Variables",
            };

            try {
                vm.Build();
            }
            catch (Exception error) {
                vm.Errors.Add(error);
            }

            return Response.View("VariableSet\\Index.html", vm);
        }
    }
}
