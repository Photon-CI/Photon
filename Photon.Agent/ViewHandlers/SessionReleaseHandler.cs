using log4net;
using Photon.Agent.Internal;
using PiServerLite.Http.Handlers;
using System;

namespace Photon.Agent.Handlers
{
    [HttpHandler("/session/release")]
    internal class SessionReleaseHandler : HttpHandler
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SessionReleaseHandler));


        public override HttpHandlerResult Post()
        {
            var sessionId = GetQuery("session");

            if (string.IsNullOrEmpty(sessionId))
                return BadRequest().SetText("'sessionId' is undefined!");

            try {
                PhotonAgent.Instance.Sessions
                    .ReleaseSessionAsync(sessionId)
                    .GetAwaiter().GetResult();

                return Ok().SetText("Ok");
            }
            catch (Exception error) {
                Log.Error($"Failed to release Session '{sessionId}'!", error);
                return Exception(error);
            }
        }
    }
}
