using log4net;
using Photon.Framework;
using Photon.Framework.Extensions;
using Photon.Library.Extensions;
using Photon.Library.HttpMessages;
using Photon.Server.Internal;
using Photon.Server.Internal.Sessions;
using PiServerLite.Http.Handlers;
using System;

namespace Photon.Server.HttpHandlers.Api.Build
{
    [HttpHandler("/api/build/start")]
    internal class BuildStartHandler : HttpHandler
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(BuildStartHandler));


        public override HttpHandlerResult Post()
        {
            var qGitRefspec = GetQuery("refspec");

            HttpBuildStartRequest startInfo = null;
            if (HttpContext.Request.ContentLength64 > 0) {
                startInfo = JsonSettings.Serializer.Deserialize<HttpBuildStartRequest>(HttpContext.Request.InputStream);
            }

            if (startInfo == null)
                return Response.BadRequest().SetText("No json request was found!");

            var _gitRefspec = qGitRefspec ?? startInfo.GitRefspec;

            try {
                if (!PhotonServer.Instance.Projects.TryGet(startInfo.ProjectId, out var project))
                    return Response.BadRequest().SetText($"Project '{startInfo.ProjectId}' was not found!");

                var projectData = PhotonServer.Instance.ProjectData.GetOrCreate(project.Id);
                var buildNumber = projectData.StartNewBuild();

                var session = new ServerBuildSession {
                    Project = project,
                    AssemblyFilename = project.AssemblyFile,
                    PreBuild = project.PreBuild,
                    TaskName = startInfo.TaskName,
                    GitRefspec = _gitRefspec,
                    BuildNumber = buildNumber,
                    Roles = startInfo.Roles,
                    Mode = startInfo.Mode,
                };

                if (!string.IsNullOrEmpty(startInfo.AssemblyFilename))
                    session.AssemblyFilename = startInfo.AssemblyFilename;

                if (!string.IsNullOrEmpty(startInfo.PreBuildCommand))
                    session.PreBuild = startInfo.PreBuildCommand;

                PhotonServer.Instance.Sessions.BeginSession(session);
                PhotonServer.Instance.Queue.Add(session);

                var response = new HttpBuildStartResponse {
                    SessionId = session.SessionId,
                };

                return Response.Json(response);
            }
            catch (Exception error) {
                Log.Error($"Failed to run Build-Task '{startInfo.TaskName}' from Project '{startInfo.ProjectId}' @ '{_gitRefspec}'!", error);
                return Response.Exception(error);
            }
        }
    }
}
