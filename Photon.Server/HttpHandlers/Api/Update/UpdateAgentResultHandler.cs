using Photon.Framework;
using Photon.Framework.Extensions;
using Photon.Library.HttpMessages;
using Photon.Server.Internal;
using Photon.Server.Internal.Sessions;
using PiServerLite.Http.Handlers;
using System;
using System.IO;

namespace Photon.Server.HttpHandlers.Update
{
    [HttpHandler("/update/result")]
    internal class UpdateResultHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var sessionId = GetQuery("session");

            try {
                if (!PhotonServer.Instance.Sessions.TryGet(sessionId, out var session))
                    return BadRequest().SetText($"Server Session '{sessionId}' was not found!");

                if (!(session is ServerUpdateSession updateSession))
                    throw new Exception($"Session '{sessionId}' is not a valid update session!");

                var response = new HttpAgentUpdateResultResponse();

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
