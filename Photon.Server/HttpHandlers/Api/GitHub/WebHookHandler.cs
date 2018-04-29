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
                string.Equals((x.GetSource() as ProjectGithubSource)?.CloneUrl, commit.RepositoryUrl, StringComparison.OrdinalIgnoreCase));

            if (project == null)
                throw new ApplicationException($"No project found matching git url '{commit.RepositoryUrl}'!");

            var source = (ProjectGithubSource)project.GetSource();
            var projectData = PhotonServer.Instance.ProjectData.GetOrCreate(project.Id);
            var buildNumber = projectData.StartNewBuild();

            var session = new ServerBuildSession {
                Project = project,
                AssemblyFilename = project.AssemblyFile,
                PreBuild = project.PreBuild,
                TaskName = source.HookTaskName,
                GitRefspec = commit.Refspec ?? commit.Sha,
                BuildNumber = buildNumber,
                Roles = source.HookTaskRoles,
            };

            PhotonServer.Instance.Sessions.BeginSession(session);
            PhotonServer.Instance.Queue.Add(session);
        }
    }
}
