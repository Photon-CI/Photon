using log4net;
using Photon.Library.Extensions;
using Photon.Library.Http;
using Photon.Library.Http.Messages;
using Photon.Server.Internal;
using Photon.Server.Internal.Security;
using Photon.Server.Internal.Sessions;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Server.ApiHandlers.Build
{
    [Secure]
    [RequiresRoles(GroupRole.BuildStart)]
    [HttpHandler("/api/build/start")]
    internal class BuildStartHandler : HttpApiHandlerAsync
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(BuildStartHandler));


        public override async Task<HttpHandlerResult> PostAsync(CancellationToken token)
        {
            var serverContext = PhotonServer.Instance.Context;

            var qProject = GetQuery("project");
            var qGitRefspec = GetQuery("refspec");
            var qTaskName = GetQuery("task");

            try {
                if (!serverContext.Projects.TryGet(qProject, out var project))
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

                var session = new ServerBuildSession(serverContext) {
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

                serverContext.Sessions.BeginSession(session);
                serverContext.Queue.Add(session);

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
