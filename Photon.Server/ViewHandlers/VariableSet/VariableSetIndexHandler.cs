using System;
using Photon.Server.ViewModels.VariableSet;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;

namespace Photon.Server.ViewHandlers.VariableSet
{
    [Secure]
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

            return Response.View("VariableSet\\Index.html", vm);
        }
    }
}
