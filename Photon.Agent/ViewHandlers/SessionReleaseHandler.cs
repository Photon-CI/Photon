using log4net;
using Newtonsoft.Json;
using Photon.Library.Extensions;
using Photon.Library.Models;
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
            var serializer = new JsonSerializer();
            var requestData = serializer.Deserialize<SessionBeginRequest>(HttpContext.Request.InputStream);

            Log.Debug($"Beginning session for Project '{requestData.ProjectName}' @ '{requestData.ReleaseVersion}'.");

            try {
                var session = Program.Sessions.BeginSession(requestData);

                var response = new SessionBeginResponse {
                    SessionId = session.Id,
                };

                return Ok()
                    .SetContentType("application/json")
                    .SetContent(s => serializer.Serialize(s, response));
            }
            catch (Exception error) {
                Log.Error($"Failed to start session for Project '{requestData.ProjectName}' @ '{requestData.ReleaseVersion}'!", error);
                return Exception(error);
            }
        }
    }
}
