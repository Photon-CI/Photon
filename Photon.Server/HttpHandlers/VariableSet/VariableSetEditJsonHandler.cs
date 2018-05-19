using Photon.Server.ViewModels.VariableSet;
using PiServerLite.Http.Handlers;

namespace Photon.Server.HttpHandlers.VariableSet
{
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
