using Photon.Agent.Internal.Security;
using Photon.Agent.ViewModels.VariableSet;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;

namespace Photon.Agent.ViewHandlers.VariableSet
{
    [Secure]
    [RequiresRoles(GroupRole.VariablesView)]
    [HttpHandler("/variables")]
    [HttpHandler("/variable/index")]
    internal class VariableSetIndexHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var vm = new VariableSetIndexVM(this) {
                PageTitle = "Photon Agent Variables",
            };

            vm.Build();

            return Response.View("VariableSet\\Index.html", vm);
        }
    }
}
