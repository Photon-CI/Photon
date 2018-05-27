using Photon.Library.Session;
using System;
using System.Linq;

namespace Photon.Agent.Internal.Session
{

    internal class AgentSessionWatch : IDisposable
    {
        public event EventHandler<SessionStatusArgs> SessionChanged;


        public AgentSessionWatch()
        {
            PhotonAgent.Instance.Sessions.SessionChanged += Session_OnChanged;
        }

        public void Dispose()
        {
            PhotonAgent.Instance.Sessions.SessionChanged -= Session_OnChanged;
        }

        public void Initialize()
        {
            var sessionList = PhotonAgent.Instance.Sessions.All
                .OrderBy(x => x.TimeCreated).ToArray();

            foreach (var session in sessionList)
                SendUpdate(session);
        }

        private void SendUpdate(AgentSessionBase session)
        {
            uint? number = null;
            string name = null;
            string projectVersion = null;
            string gitRefspec = null;
            string status;

            var projectName = session.Project?.Name;

            if (!session.IsReleased) {
                status = "running";
            }
            else if (session.Exception != null) {
                status = "failed";
            }
            else {
                status = "success";
            }

            if (session is AgentBuildSession buildSession) {
                number = buildSession.BuildNumber;
                name = "<unknown>"; // buildSession.TaskName;
                gitRefspec = buildSession.GitRefspec;
            }
            else if (session is AgentDeploySession deploySession) {
                number = deploySession.DeploymentNumber;
                name = "<unknown>"; // deploySession.ScriptName;
                projectVersion = $"{deploySession.ProjectPackageId} @{deploySession.ProjectPackageVersion}";
            }

            var data = new {
                id = session.SessionId,
                type = GetSessionType(session),
                isReleased = session.IsReleased,
                status,
                number,
                name,
                projectName,
                projectVersion,
                gitRefspec,
            };

            OnSessionChanged(data);
        }

        protected void OnSessionChanged(object data)
        {
            SessionChanged?.Invoke(this, new SessionStatusArgs(data));
        }

        private void Session_OnChanged(object sender, SessionStateEventArgs e)
        {
            SendUpdate(e.Session);
        }

        private string GetSessionType(AgentSessionBase session)
        {
            if (session is AgentBuildSession) return "build";
            if (session is AgentDeploySession) return "deploy";
            return null;
        }
    }
}
