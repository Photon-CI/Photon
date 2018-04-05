using Photon.Server.Internal;
using PiServerLite.Http.Handlers;
using System;
using System.Net;

namespace Photon.Server.HttpHandlers.Session
{
    [HttpHandler("/session/output")]
    internal class OutputHandler : HttpHandler
    {
        //private static readonly ILog Log = LogManager.GetLogger(typeof(OutputHandler));


        public override HttpHandlerResult Get()
        {
            var sessionId = GetQuery("session");
            var startPos = GetQuery<int>("start");

            try {
                if (!PhotonServer.Instance.Sessions.TryGetSession(sessionId, out var session))
                    return BadRequest().SetText($"Session '{sessionId}' not found!");

                var currentLength = session.Output.Length;

                if (currentLength <= startPos)
                    return HttpHandlerResult.Status(Context, HttpStatusCode.NotModified)
                        .SetHeader("X-Complete", session.Complete.ToString());

                var newText = session.Output.ToString()
                    .Substring(startPos);

                return Ok()
                    .SetHeader("X-Text-Pos", currentLength.ToString())
                    .SetHeader("X-Complete", session.Complete.ToString())
                    .SetText(newText);
            }
            catch (Exception error) {
                //Log.Error($"Failed to run Build-Script '{_scriptName}' from Project '{_projectId}' @ '{_gitRefspec}'!", error);
                return Exception(error);
            }
        }
    }
}
