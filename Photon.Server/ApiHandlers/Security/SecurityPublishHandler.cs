using log4net;
using Photon.Library.Extensions;
using Photon.Library.Http;
using Photon.Library.Http.Messages;
using Photon.Server.Internal;
using Photon.Server.Internal.Security;
using Photon.Server.Internal.Sessions;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;
using System;

namespace Photon.Server.ApiHandlers.Security
{
    [Secure]
    [RequiresRoles(GroupRole.SecurityEdit)]
    [HttpHandler("api/security/publish")]
    internal class SecurityPublishHandler : HttpApiHandler
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SecurityPublishHandler));


        public override HttpHandlerResult Post()
        {
            var serverContext = PhotonServer.Instance.Context;

            try {
                var session = new ServerSecurityPublishSession(serverContext);

                serverContext.Sessions.BeginSession(session);
                serverContext.Queue.Add(session);

                var response = new HttpSessionStartResponse {
                    SessionId = session.SessionId,
                };

                return Response.Json(response);
            }
            catch (Exception error) {
                Log.Error("Failed to publish security configuration!", error);
                return Response.Exception(error);
            }
        }
    }
}
