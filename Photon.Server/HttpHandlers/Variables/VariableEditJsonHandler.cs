using Photon.Server.ViewModels.Variables;
using PiServerLite.Http.Handlers;

namespace Photon.Server.HttpHandlers.Variables
{
    [HttpHandler("/variable/edit/json")]
    internal class VariableEditJsonHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var id = GetQuery("id");

            var vm = new VariablesEditJsonVM {
                PageTitle = "Photon Server Edit Variable Set JSON",
                SetId = id,
            };

            return Response.View("Variables\\EditJson.html", vm);
        }
    }
}
