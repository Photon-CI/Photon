using Photon.Library.Extensions;
using Photon.Library.HttpMessages;
using Photon.Server.Internal;
using Photon.Server.Internal.Security;
using Photon.Server.Internal.Sessions;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;
using System;

namespace Photon.Server.ApiHandlers.Agent
{
    [Secure]
    [RequiresRoles(GroupRole.AgentEdit)]
    [HttpHandler("api/agent/update/result")]
    internal class UpdateResultHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var sessionId = GetQuery("session");

            try {
                if (!PhotonServer.Instance.Sessions.TryGet(sessionId, out var session))
                    return Response.BadRequest().SetText($"Server Session '{sessionId}' was not found!");

                if (!(session is ServerUpdateSession updateSession))
                    throw new Exception($"Session '{sessionId}' is not a valid update session!");

                // TODO: Get something from session?

                var responseMessage = new HttpAgentUpdateResultResponse();

                return Response.Json(responseMessage);
            }
            catch (Exception error) {
                return Response.Exception(error);
            }
        }
    }
}
