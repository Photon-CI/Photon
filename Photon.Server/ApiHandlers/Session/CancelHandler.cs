using log4net;
using Photon.Library.Http;
using Photon.Server.Internal;
using PiServerLite.Http.Handlers;
using System;

namespace Photon.Server.ApiHandlers.Session
{
    [HttpHandler("/api/session/cancel")]
    internal class CancelHandler : HttpApiHandler
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(CancelHandler));


        public override HttpHandlerResult Post()
        {
            var serverContext = PhotonServer.Instance.Context;

            var sessionId = GetQuery("id");

            if (string.IsNullOrEmpty(sessionId))
                return Response.BadRequest().SetText("'id' is undefined!");

            if (!serverContext.Sessions.TryGet(sessionId, out var session))
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
