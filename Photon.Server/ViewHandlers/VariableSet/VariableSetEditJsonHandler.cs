using Photon.Server.Internal.Security;
using Photon.Server.ViewModels.VariableSet;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;

namespace Photon.Server.ViewHandlers.VariableSet
{
    [Secure]
    [RequiresRoles(GroupRole.VariablesEdit)]
    [HttpHandler("/variable/edit/json")]
    internal class VariableSetEditJsonHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var id = GetQuery("id");

            var vm = new VariablesEditJsonVM(this) {
                PageTitle = "Photon Server Edit Variable Set JSON",
                SetId = id,
            };

            vm.Build();

            return Response.View("VariableSet\\EditJson.html", vm);
        }
    }
}
