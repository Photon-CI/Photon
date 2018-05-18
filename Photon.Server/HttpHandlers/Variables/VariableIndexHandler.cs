using Photon.Server.ViewModels.Variables;
using PiServerLite.Http.Handlers;
using System;

namespace Photon.Server.HttpHandlers.Variables
{
    [HttpHandler("/variables")]
    [HttpHandler("/variable/index")]
    internal class VariableIndexHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var vm = new VariablesIndexVM {
                PageTitle = "Photon Server Variables",
            };

            try {
                vm.Build();
            }
            catch (Exception error) {
                vm.Errors.Add(error);
            }

            return Response.View("Variables\\Index.html", vm);
        }
    }
}
