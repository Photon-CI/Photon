using Photon.Agent.ViewModels.VariableSet;
using PiServerLite.Http.Handlers;

namespace Photon.Agent.ViewHandlers.VariableSet
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

            vm.Build();

            return Response.View("VariableSet\\Index.html", vm);
        }
    }
}
