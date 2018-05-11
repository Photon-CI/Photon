using log4net;
using Photon.Library.Extensions;
using Photon.Library.HttpMessages;
using Photon.Server.Internal;
using Photon.Server.Internal.Sessions;
using PiServerLite.Http.Handlers;
using System;

namespace Photon.Server.HttpHandlers.Api.Deploy
{
    [HttpHandler("api/deploy/start")]
    internal class DeployHandler : HttpHandler
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(DeployHandler));


        public override HttpHandlerResult Post()
        {
            var projectId = GetQuery("project");
            var projectPackageId = GetQuery("package");
            var projectPackageVersion = GetQuery("version");
            var environmentName = GetQuery("env");

            if (string.IsNullOrWhiteSpace(projectId))
                return Response.BadRequest().SetText("'project' is undefined!");

            if (string.IsNullOrWhiteSpace(projectPackageId))
                return Response.BadRequest().SetText("'package' is undefined!");

            if (string.IsNullOrWhiteSpace(projectPackageVersion))
                return Response.BadRequest().SetText("'version' is undefined!");

            try {
                if (!PhotonServer.Instance.ProjectPackages.TryGet(projectPackageId, projectPackageVersion, out var packageFilename))
                    return Response.BadRequest().SetText($"Project Package '{projectPackageId}.{projectPackageVersion}' was not found!");

                if (!PhotonServer.Instance.Projects.TryGet(projectId, out var project))
                    return Response.BadRequest().SetText($"Project '{projectId}' was not found!");

                var projectData = PhotonServer.Instance.ProjectData.GetOrCreate(project.Id);
                var deploymentNumber = projectData.StartNewBuild();

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
