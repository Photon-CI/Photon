using Photon.Server.ViewModels.Agent;
using PiServerLite.Http.Handlers;

namespace Photon.Server.HttpHandlers.Agent
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

            return Response.View("Agent\\EditJson.html", vm);
        }
    }
}
