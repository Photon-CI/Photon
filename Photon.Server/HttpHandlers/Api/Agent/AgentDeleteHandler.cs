using Photon.Server.Internal;
using PiServerLite.Http.Handlers;

namespace Photon.Server.HttpHandlers.Api.Agent
{
    [HttpHandler("api/agent/delete")]
    internal class AgentDeleteHandler : HttpHandler
    {
        public override HttpHandlerResult Post()
        {
            var qId = GetQuery("id");

            if (string.IsNullOrEmpty(qId))
                return BadRequest().SetText("'id' is undefined!");

            PhotonServer.Instance.Agents.Remove(qId);

            return Ok();
        }
    }
}
