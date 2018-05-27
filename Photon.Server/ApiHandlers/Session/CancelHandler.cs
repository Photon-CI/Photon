using log4net;
using Photon.Server.Internal;
using PiServerLite.Http.Handlers;
using System;

namespace Photon.Server.ApiHandlers.Session
{
    [HttpHandler("/api/session/cancel")]
    internal class CancelHandler : HttpHandler
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(CancelHandler));


        public override HttpHandlerResult Post()
        {
            var sessionId = GetQuery("id");

            if (string.IsNullOrEmpty(sessionId))
                return Response.BadRequest().SetText("'id' is undefined!");

            if (!PhotonServer.Instance.Sessions.TryGet(sessionId, out var session))
                return Response.BadRequest().SetText($"Session '{sessionId}' was not found!");

            try {
                session.Abort();
            }
            catch (Exception error) {
                Log.Warn($"Failed to cancel session '{sessionId}'!", error);
                throw;
            }

            return Response.Ok()
                .SetContentType("text/plain");
        }
    }
}
