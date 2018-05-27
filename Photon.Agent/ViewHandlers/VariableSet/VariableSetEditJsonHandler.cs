using Photon.Agent.ViewModels.VariableSet;
using PiServerLite.Http.Handlers;

namespace Photon.Agent.HttpHandlers.VariableSet
{
    [HttpHandler("/variable/edit/json")]
    internal class VariableEditJsonHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var id = GetQuery("id");

            var vm = new VariableSetEditJsonVM {
                PageTitle = "Photon Agent Edit Variable Set JSON",
                SetId = id,
            };

            return Response.View("VariableSet\\EditJson.html", vm);
        }
    }
}
