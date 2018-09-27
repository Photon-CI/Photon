using Photon.Agent.Internal;
using Photon.Agent.Internal.Session;
using PiServerLite.Http.Handlers;
using System;

namespace Photon.Agent.ViewModels.Session
{
    internal class SessionDetailsVM : AgentViewModel
    {
        public string SessionId {get; set;}
        public string SessionTitle {get; set;}
        public string SessionStatus {get; set;}


        public SessionDetailsVM(IHttpHandler handler) : base(handler) {}

        protected override void OnBuild()
        {
            base.OnBuild();

            if (!PhotonAgent.Instance.Context.Sessions.TryGet(SessionId, out var session))
                throw new ApplicationException($"Session '{SessionId}' not found!");

            SessionTitle = null;
            if (session is AgentBuildSession buildSession) {
                var color = !session.IsReleased ? "text-primary"
                    : session.Exception != null ? "text-danger"
                    : "text-success";

                SessionStatus = $"fas fa-cubes {color}";
                SessionTitle = $"{session.Project.Name} - Build #{buildSession.BuildNumber} - {buildSession.TaskName} @{buildSession.GitRefspec}";
            }
            else if (session is AgentDeploySession deploySession) {
                var color = !session.IsReleased ? "text-primary"
                    : session.Exception != null ? "text-danger"
                    : "text-success";

                SessionStatus = $"fas fa-cloud-download-alt {color}";
                SessionTitle = $"{session.Project.Name} - Deployment #{deploySession.DeploymentNumber} - {deploySession.ProjectPackageId} @{deploySession.ProjectPackageVersion}";
            }
        }
    }
}
