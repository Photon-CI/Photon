using Photon.Server.ViewModels.VariableSet;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;

namespace Photon.Server.ViewHandlers.VariableSet
{
    [Secure]
    [HttpHandler("/variable/edit/json")]
    internal class VariableSetEditJsonHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var id = GetQuery("id");

            var vm = new VariablesEditJsonVM {
                PageTitle = "Photon Server Edit Variable Set JSON",
                SetId = id,
            };

            return Response.View("VariableSet\\EditJson.html", vm);
        }
    }
}
