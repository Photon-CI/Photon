using Photon.Server.ViewModels.Agents;
using PiServerLite.Http.Handlers;

namespace Photon.Server.HttpHandlers.Agents
{
    [HttpHandler("/agent/edit/json")]
    internal class AgentEditJsonHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var id = GetQuery("id");

            var vm = new AgentEditJsonVM {
                PageTitle = "Photon Server Edit Agent JSON",
                AgentId = id,
            };

            return Response.View("Agents\\EditJson.html", vm);
        }
    }
}
