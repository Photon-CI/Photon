using Photon.Server.Internal;
using PiServerLite.Http.Handlers;
using System;
using System.Net;

namespace Photon.Server.HttpHandlers.Api.Session
{
    [HttpHandler("api/session/output")]
    internal class OutputHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var sessionId = GetQuery("session");
            var startPos = GetQuery<int>("start");

            try {
                if (!PhotonServer.Instance.Sessions.TryGet(sessionId, out var session))
                    return Response.BadRequest().SetText($"Session '{sessionId}' not found!");

                var currentLength = session.Output.Length;

                if (currentLength <= startPos)
                    return Response.Status(HttpStatusCode.NotModified)
                        .SetHeader("X-Complete", session.IsComplete.ToString());

                var newText = session.Output.ToString()
                    .Substring(startPos);

                return Response.Ok()
                    .SetHeader("X-Text-Pos", currentLength.ToString())
                    .SetHeader("X-Complete", session.IsComplete.ToString())
                    .SetText(newText);
            }
            catch (Exception error) {
                return Response.Exception(error);
            }
        }
    }
}
