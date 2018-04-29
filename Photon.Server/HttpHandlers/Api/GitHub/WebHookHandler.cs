using Photon.Framework.Projects;
using Photon.Server.Internal;
using Photon.Server.Internal.GitHub;
using Photon.Server.Internal.Sessions;
using PiServerLite.Http.Handlers;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Photon.Server.HttpHandlers.Api.GitHub
{
    [HttpHandler("api/github/webhook")]
    internal class WebHookHandler : HttpHandlerAsync
    {
        public override async Task<HttpHandlerResult> PostAsync()
        {
            var eventType = HttpContext.Request.Headers["X-GitHub-Event"];
            //var deliveryId = HttpContext.Request.Headers["X-GitHub-Delivery"];
            //var signature = HttpContext.Request.Headers["X-Hub-Signature"];

            if (string.IsNullOrEmpty(eventType))
                return BadRequest().SetText("GitHub Webhook Event-Type is undefined!");

            var json = await As.TextAsync();
            var commit = HookEventHandler.ParseEvent(eventType, json);

            if (commit != null)
                StartBuild(commit);

            return Ok();
        }

        private void StartBuild(GithubCommit commit)
        {
            var project = PhotonServer.Instance.Projects.FirstOrDefault(x =>
                string.Equals((x.Source as ProjectGithubSource)?.CloneUrl, commit.RepositoryUrl, StringComparison.OrdinalIgnoreCase));

            if (project == null)
                throw new ApplicationException($"No project found matching git url '{commit.RepositoryUrl}'!");

            var source = (ProjectGithubSource)project.Source;
            var projectData = PhotonServer.Instance.ProjectData.GetOrCreate(project.Id);
            var buildNumber = projectData.StartNewBuild();

            var session = new ServerBuildSession {
                Project = project,
                AssemblyFilename = project.AssemblyFile,
                PreBuild = project.PreBuild,
                TaskName = source.HookTaskName,
                GitRefspec = commit.Refspec,
                BuildNumber = buildNumber,
                Roles = source.HookTaskRoles,
            };

            if (!string.IsNullOrEmpty(commit.Sha)) {
                var su = new CommitStatusUpdater {
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

            PhotonServer.Instance.Sessions.BeginSession(session);
            PhotonServer.Instance.Queue.Add(session);
        }
    }
}
