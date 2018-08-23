using Photon.Library.Http;
using Photon.Server.Internal;
using Photon.Server.Internal.Security;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;

namespace Photon.Server.ApiHandlers.Agent
{
    [Secure]
    [RequiresRoles(GroupRole.AgentEdit)]
    [HttpHandler("api/agent/delete")]
    internal class AgentDeleteHandler : HttpApiHandler
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
