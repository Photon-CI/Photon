using log4net;
using Photon.Framework.Projects;
using Photon.Library.GitHub;
using Photon.Library.Http;
using Photon.Server.Internal;
using Photon.Server.Internal.GitHub;
using Photon.Server.Internal.Sessions;
using PiServerLite.Http.Handlers;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Server.ApiHandlers.GitHub
{
    [HttpHandler("api/github/webhook")]
    internal class WebHookHandler : HttpApiHandlerAsync
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(WebHookHandler));


        public override async Task<HttpHandlerResult> PostAsync(CancellationToken token)
        {
            var eventType = HttpContext.Request.Headers["X-GitHub-Event"];
            //var deliveryId = HttpContext.Request.Headers["X-GitHub-Delivery"];
            //var signature = HttpContext.Request.Headers["X-Hub-Signature"];

            if (string.IsNullOrEmpty(eventType))
                return Response.BadRequest().SetText("GitHub Webhook Event-Type is undefined!");

            var json = await Request.TextAsync();
            GithubCommit commit;

            try {
                commit = HookEventHandler.ParseEvent(eventType, json);
            }
            catch (Exception error) {
                var subtext = json;
                if (subtext.Length > 96) {
                    subtext = subtext.Substring(0, 96)+$"...+{subtext.Length-96})";
                }
                Log.Error($"Failed to parse GitHub webhook event! [{subtext}]", error);
                throw;
            }

            if (commit != null)
                await StartBuild(commit);

            return Response.Ok();
        }

        private async Task StartBuild(GithubCommit commit)
        {
            var serverContext = PhotonServer.Instance.Context;

            var project = serverContext.Projects.All.FirstOrDefault(x =>
                string.Equals((x.Description.Source as ProjectGithubSource)?.CloneUrl, commit.RepositoryUrl, StringComparison.OrdinalIgnoreCase));

            if (project == null)
                throw new ApplicationException($"No project found matching git url '{commit.RepositoryUrl}'!");

            var source = (ProjectGithubSource)project.Description.Source;
            var build = await project.StartNewBuild();
            build.TaskName = source.HookTaskName;
            build.TaskRoles = source.HookTaskRoles;
            build.PreBuildCommand = project.Description.PreBuild;
            build.AssemblyFilename = project.Description.AssemblyFile;
            build.GitRefspec = commit.Refspec;
            build.Commit = commit;

            var session = new ServerBuildSession(serverContext) {
                Project = project.Description,
                AssemblyFilename = project.Description.AssemblyFile,
                PreBuild = project.Description.PreBuild,
                TaskName = source.HookTaskName,
                GitRefspec = commit.Refspec,
                Build = build,
                Roles = source.HookTaskRoles,
                Commit = commit,
            };

            build.ServerSessionId = session.SessionId;

            if (source.NotifyOrigin == NotifyOrigin.Server && !string.IsNullOrEmpty(commit.Sha))
                ApplyGithubNotification(session, source, commit);

            serverContext.Sessions.BeginSession(session);
            serverContext.Queue.Add(session);
        }

        private void ApplyGithubNotification(ServerBuildSession session, ProjectGithubSource source, GithubCommit commit)
        {
            var su = new CommitStatusUpdater {
                Username = source.Username,
                Password = source.Password,
                StatusUrl = commit.StatusesUrl,
                Sha = commit.Sha,
            };

            session.PreBuildEvent += (o, e) => {
                var status = new CommitStatus {
                    State = CommitStates.Pending,
                    Context = "Photon",
                    Description = "Build in progress..."
                };

                su.Post(status).GetAwaiter().GetResult();
            };

            session.PostBuildEvent += (o, e) => {
                var status = new CommitStatus {
                    Context = "Photon",
                };

                if (session.Result.Cancelled) {
                    status.State = CommitStates.Error;
                    status.Description = "The build was cancelled.";
                }
                else if (session.Result.Successful) {
                    status.State = CommitStates.Success;
                    status.Description = $"Build Successful. {session.Result.Message}";
                }
                else {
                    status.State = CommitStates.Failure;
                    status.Description = $"Build Failed! {session.Result.Message}";
                }

                su.Post(status).GetAwaiter().GetResult();
            };
        }
    }
}
