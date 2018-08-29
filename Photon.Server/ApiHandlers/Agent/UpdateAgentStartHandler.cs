using log4net;
using Photon.Framework.Tools;
using Photon.Library.Extensions;
using Photon.Library.Http;
using Photon.Library.Http.Messages;
using Photon.Server.Internal;
using Photon.Server.Internal.Sessions;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Server.ApiHandlers.Agent
{
    [Secure]
    [HttpHandler("api/agent/update/start")]
    internal class UpdateAgentStartHandler : HttpApiHandlerAsync
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(UpdateAgentStartHandler));


        public override async Task<HttpHandlerResult> PostAsync(CancellationToken token)
        {
            var agentIds = GetQuery("agents");

            var updateDirectory = Path.Combine(Configuration.Directory, "Updates");
            var updateFilename = Path.Combine(updateDirectory, "Photon.Agent.msi");

            PathEx.CreatePath(updateDirectory);

            using (var fileStream = File.Open(updateFilename, FileMode.Create, FileAccess.Write)) {
                await HttpContext.Request.InputStream.CopyToAsync(fileStream);
            }

            try {
                var session = new ServerUpdateSession {
                    UpdateFilename = updateFilename,
                };

                if (!string.IsNullOrEmpty(agentIds))
                    session.AgentIds = ParseNames(agentIds).OrderBy(x => x).ToArray();

                PhotonServer.Instance.Sessions.BeginSession(session);
                PhotonServer.Instance.Queue.Add(session);

                var response = new HttpAgentUpdateStartResponse {
                    SessionId = session.SessionId,
                };

                return Response.Json(response);
            }
            catch (Exception error) {
                Log.Error("Failed to run Update-Task!", error);
                return Response.Exception(error);
            }
        }

        private static IEnumerable<string> ParseNames(string names)
        {
            return names.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim());
        }
    }
}
