using log4net;
using Photon.Framework;
using Photon.Framework.Extensions;
using Photon.Library.Extensions;
using Photon.Library.HttpMessages;
using Photon.Server.Internal;
using Photon.Server.Internal.Sessions;
using PiServerLite.Http.Handlers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Server.ApiHandlers.Build
{
    [HttpHandler("/api/build/start")]
    internal class BuildStartHandler : HttpHandlerAsync
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(BuildStartHandler));


        public override async Task<HttpHandlerResult> PostAsync(CancellationToken token)
        {
            var qProject = GetQuery("project");
            var qGitRefspec = GetQuery("refspec");
            var qTaskName = GetQuery("task");

            HttpBuildStartRequest startInfo = null;
            if (HttpContext.Request.ContentLength64 > 0) {
                startInfo = JsonSettings.Serializer.Deserialize<HttpBuildStartRequest>(HttpContext.Request.InputStream);
            }

            if (startInfo == null)
                return Response.BadRequest().SetText("No json request was found!");

            var _projectId = qProject ?? startInfo.ProjectId;
            var _gitRefspec = qGitRefspec ?? startInfo.GitRefspec;
            var _taskName = qTaskName ?? startInfo.TaskName;

            try {
                if (!PhotonServer.Instance.Projects.TryGet(_projectId, out var project))
                    return Response.BadRequest().SetText($"Project '{_projectId}' was not found!");

                var build = await project.StartNewBuild();

                var session = new ServerBuildSession {
                    Project = project.Description,
                    AssemblyFilename = project.Description.AssemblyFile,
                    PreBuild = project.Description.PreBuild,
                    TaskName = _taskName,
                    GitRefspec = _gitRefspec,
                    Build = build,
                    Roles = startInfo.Roles,
                    Mode = startInfo.Mode,
                };

                build.ServerSessionId = session.SessionId;

                if (!string.IsNullOrEmpty(startInfo.AssemblyFilename))
                    session.AssemblyFilename = startInfo.AssemblyFilename;

                if (!string.IsNullOrEmpty(startInfo.PreBuildCommand))
                    session.PreBuild = startInfo.PreBuildCommand;

                PhotonServer.Instance.Sessions.BeginSession(session);
                PhotonServer.Instance.Queue.Add(session);

                var response = new HttpBuildStartResponse {
                    SessionId = session.SessionId,
                    BuildNumber = session.Build.Number,
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
