using Photon.Library.Extensions;
using Photon.Library.Http;
using Photon.Library.Http.Messages;
using Photon.Server.Internal;
using Photon.Server.Internal.Sessions;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;
using System;

namespace Photon.Server.ApiHandlers.Deploy
{
    [Secure]
    [HttpHandler("api/deploy/result")]
    internal class DeployResultHandler : HttpApiHandler
    {
        public override HttpHandlerResult Get()
        {
            var sessionId = GetQuery("session");
            var serverContext = PhotonServer.Instance.Context;

            try {
                if (!serverContext.Sessions.TryGet(sessionId, out var session))
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
