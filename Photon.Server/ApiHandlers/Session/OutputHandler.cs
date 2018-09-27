using Photon.Server.Internal;
using PiServerLite.Http.Handlers;
using System;
using System.Net;

namespace Photon.Server.ApiHandlers.Session
{
    [HttpHandler("api/session/output")]
    internal class OutputHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var serverContext = PhotonServer.Instance.Context;

            var sessionId = GetQuery("session");
            var startPos = GetQuery<int>("start");

            try {
                if (!serverContext.Sessions.TryGet(sessionId, out var session))
                    return Response.BadRequest().SetText($"Session '{sessionId}' not found!");

                var currentLength = session.Output.Length;

                if (currentLength <= startPos)
                    return Response.Status(HttpStatusCode.NotModified)
                        .SetHeader("X-Complete", session.IsComplete.ToString());

                var newText = session.Output.GetString()
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
