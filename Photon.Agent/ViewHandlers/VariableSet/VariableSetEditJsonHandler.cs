using Photon.Agent.Internal.Security;
using Photon.Agent.ViewModels.VariableSet;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;

namespace Photon.Agent.ViewHandlers.VariableSet
{
    [Secure]
    [RequiresRoles(GroupRole.VariablesEdit)]
    [HttpHandler("/variable/edit/json")]
    internal class VariableEditJsonHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var id = GetQuery("id");

            var vm = new VariableSetEditJsonVM(this) {
                PageTitle = "Photon Agent Edit Variable Set JSON",
                SetId = id,
            };

            vm.Build();

            return Response.View("VariableSet\\EditJson.html", vm);
        }
    }
}
