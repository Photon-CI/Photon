using Photon.Library.Extensions;
using Photon.Library.Http;
using Photon.Library.Http.Messages;
using Photon.Server.Internal;
using Photon.Server.Internal.Sessions;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;
using System;
using System.Linq;

namespace Photon.Server.ApiHandlers.Build
{
    [Secure]
    [HttpHandler("api/build/result")]
    internal class BuildResultHandler : HttpApiHandler
    {
        public override HttpHandlerResult Get()
        {
            var sessionId = GetQuery("session");

            try {
                if (!PhotonServer.Instance.Sessions.TryGet(sessionId, out var session))
                    return Response.BadRequest().SetText($"Server Session '{sessionId}' was not found!");

                if (!(session is ServerBuildSession buildSession))
                    throw new Exception($"Session '{sessionId}' is not a valid build session!");

                var response = new HttpBuildResultResponse {
                    BuildNumber = buildSession.Build.Number,
                    Result = buildSession.Result,
                    ProjectPackages = buildSession.Packages.PushedProjectPackages
                        .Select(x => new HttpPackageReference(x)).ToArray(),
                    ApplicationPackages = buildSession.Packages.PushedApplicationPackages
                        .Select(x => new HttpPackageReference(x)).ToArray(),
                };

                return Response.Json(response);
            }
            catch (Exception error) {
                return Response.Exception(error);
            }
        }
    }
}
