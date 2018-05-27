using System;
using Photon.Library.Extensions;
using Photon.Library.HttpMessages;
using Photon.Server.Internal;
using Photon.Server.Internal.Sessions;
using PiServerLite.Http.Handlers;

namespace Photon.Server.ApiHandlers.Deploy
{
    [HttpHandler("api/deploy/result")]
    internal class DeployResultHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var sessionId = GetQuery("session");

            try {
                if (!PhotonServer.Instance.Sessions.TryGet(sessionId, out var session))
                    return Response.BadRequest().SetText($"Server Session '{sessionId}' was not found!");

                if (!(session is ServerDeploySession deploySession))
                    throw new Exception($"Session '{sessionId}' is not a valid deploy session!");

                var response = new HttpDeployResultResponse {
                    Result = deploySession.Result,
                };

                return Response.Json(response);
            }
            catch (Exception error) {
                return Response.Exception(error);
            }
        }
    }
}
