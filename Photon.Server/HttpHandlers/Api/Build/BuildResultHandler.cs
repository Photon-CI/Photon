using Photon.Framework;
using Photon.Framework.Extensions;
using Photon.Library.HttpMessages;
using Photon.Server.Internal;
using Photon.Server.Internal.Sessions;
using PiServerLite.Http.Handlers;
using System;
using System.IO;
using System.Linq;

namespace Photon.Server.HttpHandlers.Api.Build
{
    [HttpHandler("api/build/result")]
    internal class BuildResultHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var sessionId = GetQuery("session");

            try {
                if (!PhotonServer.Instance.Sessions.TryGet(sessionId, out var session))
                    return BadRequest().SetText($"Server Session '{sessionId}' was not found!");

                if (!(session is ServerBuildSession buildSession))
                    throw new Exception($"Session '{sessionId}' is not a valid build session!");

                var response = new HttpBuildResultResponse {
                    BuildNumber = buildSession.BuildNumber,
                    Result = buildSession.Result,
                    ProjectPackages = buildSession.PushedProjectPackages
                        .Select(x => new HttpPackageReference(x)).ToArray(),
                };

                var memStream = new MemoryStream();

                try {
                    JsonSettings.Serializer.Serialize(memStream, response, true);
                }
                catch {
                    memStream.Dispose();
                    throw;
                }

                return Ok()
                    .SetContentType("application/json")
                    .SetContent(memStream);
            }
            catch (Exception error) {
                return Exception(error);
            }
        }
    }
}
