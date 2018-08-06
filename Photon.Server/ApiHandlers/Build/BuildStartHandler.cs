using log4net;
using Photon.Library.Extensions;
using Photon.Library.HttpMessages;
using Photon.Server.Internal;
using Photon.Server.Internal.Sessions;
using PiServerLite.Http.Handlers;
using System;
using System.Linq;
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

            //HttpBuildStartRequest startInfo = null;
            //if (HttpContext.Request.ContentLength64 > 0) {
            //    startInfo = JsonSettings.Serializer.Deserialize<HttpBuildStartRequest>(HttpContext.Request.InputStream);
            //}

            //if (startInfo == null)
            //    return Response.BadRequest().SetText("No json request was found!");

            //var _projectId = qProject ?? startInfo.ProjectId;
            //var _gitRefspec = qGitRefspec ?? startInfo.GitRefspec;
            //var _taskName = qTaskName ?? startInfo.TaskName;

            try {
                if (!PhotonServer.Instance.Projects.TryGet(qProject, out var project))
                    return Response.BadRequest().SetText($"Project '{qProject}' was not found!");

                var buildTask = project.Description.BuildTasks
                    .FirstOrDefault(x => string.Equals(x.Name, qTaskName, StringComparison.OrdinalIgnoreCase));

                if (buildTask == null)
                    return Response.BadRequest().SetText($"Build-Task '{qTaskName}' was not found!");

                var build = await project.StartNewBuild();
                build.TaskName = buildTask.Name;
                build.TaskRoles = buildTask.Roles.ToArray();
                build.PreBuildCommand =  project.Description.PreBuild;
                build.AssemblyFilename = project.Description.AssemblyFile;
                build.GitRefspec = qGitRefspec ?? buildTask.GitRefspec;

                var session = new ServerBuildSession {
                    Project = project.Description,
                    AssemblyFilename = project.Description.AssemblyFile,
                    PreBuild = project.Description.PreBuild,
                    TaskName = buildTask.Name,
                    GitRefspec = qGitRefspec ?? buildTask.GitRefspec,
                    Build = build,
                    Roles = buildTask.Roles.ToArray(),
                    Mode = AgentStartModes.Any, // TODO: AgentStartMode.Parse(buildTask.AgentMode),
                };

                build.ServerSessionId = session.SessionId;

                PhotonServer.Instance.Sessions.BeginSession(session);
                PhotonServer.Instance.Queue.Add(session);

                var response = new HttpBuildStartResponse {
                    SessionId = session.SessionId,
                    BuildNumber = session.Build.Number,
                };

                return Response.Json(response);
            }
            catch (Exception error) {
                Log.Error($"Failed to run Build-Task '{qTaskName}' from Project '{qProject}' @ '{qGitRefspec}'!", error);
                return Response.Exception(error);
            }
        }
    }
}
