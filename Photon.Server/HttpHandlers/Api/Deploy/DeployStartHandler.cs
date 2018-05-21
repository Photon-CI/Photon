using log4net;
using Photon.Framework.Packages;
using Photon.Library.Extensions;
using Photon.Library.HttpMessages;
using Photon.Server.Internal;
using Photon.Server.Internal.Sessions;
using PiServerLite.Http.Handlers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Server.HttpHandlers.Api.Deploy
{
    [HttpHandler("api/deploy/start")]
    internal class DeployHandler : HttpHandlerAsync
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(DeployHandler));


        public override async Task<HttpHandlerResult> PostAsync(CancellationToken token)
        {
            var projectPackageId = GetQuery("package");
            var projectPackageVersion = GetQuery("version");
            var environmentName = GetQuery("env");

            if (string.IsNullOrWhiteSpace(projectPackageId))
                return Response.BadRequest().SetText("'package' is undefined!");

            if (string.IsNullOrWhiteSpace(projectPackageVersion))
                return Response.BadRequest().SetText("'version' is undefined!");

            try {
                if (!PhotonServer.Instance.ProjectPackages.TryGet(projectPackageId, projectPackageVersion, out var packageFilename))
                    return Response.BadRequest().SetText($"Project Package '{projectPackageId}.{projectPackageVersion}' was not found!");

                var metadata = await ProjectPackageTools.GetMetadataAsync(packageFilename);
                var projectId = metadata.ProjectId;

                if (!PhotonServer.Instance.Projects.TryGet(projectId, out var project))
                    return Response.BadRequest().SetText($"Project '{projectId}' was not found!");

                var projectData = PhotonServer.Instance.ProjectData.GetOrCreate(projectId);
                var deploymentNumber = projectData.StartNewDeployment();

                var session = new ServerDeploySession {
                    Project = project,
                    DeploymentNumber = deploymentNumber,
                    ProjectPackageId = projectPackageId,
                    ProjectPackageVersion = projectPackageVersion,
                    ProjectPackageFilename = packageFilename,
                    EnvironmentName = environmentName,
                };

                PhotonServer.Instance.Sessions.BeginSession(session);
                PhotonServer.Instance.Queue.Add(session);

                var response = new HttpDeployStartResponse {
                    SessionId = session.SessionId,
                };

                return Response.Json(response);
            }
            catch (Exception error) {
                Log.Error($"Deployment of Project Package '{projectPackageId}.{projectPackageVersion}' has failed!", error);
                return Response.Exception(error);
            }
        }
    }
}
