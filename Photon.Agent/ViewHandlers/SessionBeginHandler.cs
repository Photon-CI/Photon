using System;
using System.IO;
using log4net;
using Newtonsoft.Json;
using Photon.Agent.Internal;
using Photon.Framework.Extensions;
using Photon.Library.Messages;
using PiServerLite.Http.Handlers;

namespace Photon.Agent.ViewHandlers
{
    [HttpHandler("/session/begin")]
    internal class SessionBeginHandler : HttpHandler
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SessionBeginHandler));


        public override HttpHandlerResult Post()
        {
            try {
                var session = new AgentSession();
                //...

                PhotonAgent.Instance.Sessions.BeginSession(session);

                var response = new BuildSessionBeginResponse {
                    SessionId = session.SessionId,
                };

                var memStream = new MemoryStream();

                try {
                    new JsonSerializer()
                        .Serialize(memStream, response, true);
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
                Log.Error($"Failed to begin session!", error);
                return Exception(error);
            }
        }
    }
}
