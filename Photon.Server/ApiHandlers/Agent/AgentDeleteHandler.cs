using Photon.Server.Internal;
using PiServerLite.Http.Handlers;

namespace Photon.Server.ApiHandlers.Agent
{
    [HttpHandler("api/agent/delete")]
    internal class AgentDeleteHandler : HttpHandler
    {
        public override HttpHandlerResult Post()
        {
            var qId = GetQuery("id");

            if (string.IsNullOrEmpty(qId))
                return Response.BadRequest().SetText("'id' is undefined!");

            PhotonServer.Instance.Agents.Remove(qId);

            return Response.Ok();
        }
    }
}
