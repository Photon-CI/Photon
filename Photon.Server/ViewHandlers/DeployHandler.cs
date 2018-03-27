using log4net;
using Newtonsoft.Json;
using Photon.Library.Extensions;
using Photon.Library.Models;
using Photon.Server.Internal;
using PiServerLite.Http.Handlers;
using System;

namespace Photon.Server.Handlers
{
    [HttpHandler("/deploy")]
    internal class DeployHandler : HttpHandler
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(DeployHandler));


        public override HttpHandlerResult Post()
        {
            var serializer = new JsonSerializer();
            var requestData = serializer.Deserialize<SessionBeginRequest>(HttpContext.Request.InputStream);

            Log.Debug($"Beginning session for Project '{requestData.ProjectName}' @ '{requestData.ReleaseVersion}'.");

            try {
                var session = new ServerDeploySession();
                //...

                Program.Sessions.BeginSession(session);
                Program.Queue.Add(session);

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
