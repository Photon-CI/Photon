using log4net;
using Photon.Framework;
using Photon.Framework.Extensions;
using Photon.Library.HttpMessages;
using Photon.Server.Internal;
using Photon.Server.Internal.Sessions;
using PiServerLite.Http.Handlers;
using System;
using System.IO;

namespace Photon.Server.HttpHandlers.Deploy
{
    [HttpHandler("/deploy/start")]
    internal class DeployHandler : HttpHandler
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(DeployHandler));


        public override HttpHandlerResult Post()
        {
            var projectPackageId = GetQuery("id");
            var projectPackageVersion = GetQuery("version");

            if (string.IsNullOrWhiteSpace(projectPackageId))
                return BadRequest().SetText("'id' is undefined!");

            if (string.IsNullOrWhiteSpace(projectPackageVersion))
                return BadRequest().SetText("'version' is undefined!");

            try {
                if (!PhotonServer.Instance.ProjectPackages.TryGet(projectPackageId, projectPackageVersion, out var packageFilename))
                    return BadRequest().SetText($"Project Package '{projectPackageId}.{projectPackageVersion}' was not found!");

                var session = new ServerDeploySession {
                    ProjectPackageId = projectPackageId,
                    ProjectPackageVersion = projectPackageVersion,
                    ProjectPackageFilename = packageFilename,
                };

                PhotonServer.Instance.Sessions.BeginSession(session);
                PhotonServer.Instance.Queue.Add(session);

                var response = new HttpDeployStartResponse {
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
                    //.SetContent(s => new JsonSerializer().Serialize(s, response));
            }
            catch (Exception error) {
                Log.Error($"Deployment of Project Package '{projectPackageId}.{projectPackageVersion}' has failed!", error);
                return Exception(error);
            }
        }
    }
}
