using log4net;
using Photon.Framework;
using Photon.Framework.Extensions;
using Photon.Library.HttpMessages;
using Photon.Server.Internal;
using Photon.Server.Internal.Sessions;
using PiServerLite.Http.Handlers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Photon.Server.HttpHandlers.Api.Agent
{
    [HttpHandler("api/agent/update/start")]
    internal class UpdateAgentStartHandler : HttpHandler
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(UpdateAgentStartHandler));


        public override HttpHandlerResult Post()
        {
            var _names = GetQuery("names");

            try {
                var session = new ServerUpdateSession();

                if (!string.IsNullOrEmpty(_names))
                    session.AgentNames = ParseNames(_names).ToArray();

                PhotonServer.Instance.Sessions.BeginSession(session);
                PhotonServer.Instance.Queue.Add(session);

                var response = new HttpAgentUpdateStartResponse {
                    SessionId = session.SessionId,
                };

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
                Log.Error("Failed to run Update-Task!", error);
                return Exception(error);
            }
        }

        private IEnumerable<string> ParseNames(string names)
        {
            return names.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim());
        }
    }
}
