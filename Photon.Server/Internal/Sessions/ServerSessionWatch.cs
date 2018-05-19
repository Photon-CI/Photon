using Photon.Library.Session;
using System;

namespace Photon.Server.Internal.Sessions
{

    internal class ServerSessionWatch : IDisposable
    {
        public event EventHandler<SessionStatusArgs> SessionChanged;


        public ServerSessionWatch()
        {
            PhotonServer.Instance.Sessions.SessionStarted += OnSessionStartedStopped;
            PhotonServer.Instance.Sessions.SessionReleased += OnSessionStartedStopped;
        }

        public void Dispose()
        {
            PhotonServer.Instance.Sessions.SessionStarted -= OnSessionStartedStopped;
            PhotonServer.Instance.Sessions.SessionReleased -= OnSessionStartedStopped;
        }

        public void Initialize()
        {
            foreach (var session in PhotonServer.Instance.Sessions.Active)
                SendUpdate(session);
        }

        public void OnSessionStartedStopped(object sender, SessionStateEventArgs e)
        {
            SendUpdate(e.Session);
        }

        private void SendUpdate(ServerSessionBase session)
        {
            uint? number = null;
            string name = null;
            string projectName = null;
            string projectVersion = null;
            string gitRefspec = null;

            if (session is ServerBuildSession buildSession) {
                number = buildSession.BuildNumber;
                name = buildSession.TaskName;
                projectName = buildSession.Project?.Name;
                gitRefspec = buildSession.GitRefspec;
            }
            else if (session is ServerDeploySession deploySession) {
                number = deploySession.DeploymentNumber;
                name = deploySession.ScriptName;
                projectName = deploySession.Project?.Name;
                projectVersion = $"{deploySession.ProjectPackageId} @{deploySession.ProjectPackageVersion}";
            }
            else if (session is ServerUpdateSession updateSession) {
                projectName = "Update Agents";
                projectVersion = updateSession.AgentIds != null
                    ? string.Join(", ", updateSession.AgentIds)
                    : "<all>";
            }

            var data = new {
                id = session.SessionId,
                type = GetSessionType(session),
                isReleased = session.IsReleased,
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

        private string GetSessionType(ServerSessionBase session)
        {
            if (session is ServerBuildSession) return "build";
            if (session is ServerDeploySession) return "deploy";
            if (session is ServerUpdateSession) return "update";
            return null;
        }
    }
}
