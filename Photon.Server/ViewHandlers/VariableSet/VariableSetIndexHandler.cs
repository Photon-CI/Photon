using Photon.Server.Internal.Security;
using Photon.Server.ViewModels.VariableSet;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;

namespace Photon.Server.ViewHandlers.VariableSet
{
    [Secure]
    [RequiresRoles(GroupRole.VariablesView)]
    [HttpHandler("/variables")]
    [HttpHandler("/variable/index")]
    internal class VariableIndexHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var vm = new VariablesIndexVM(this) {
                PageTitle = "Photon Server Variables",
            };

            vm.Build();

            return Response.View("VariableSet\\Index.html", vm);
        }
    }
}
