using Photon.Library.Session;
using System;
using System.Linq;

namespace Photon.Server.Internal.Sessions
{

    internal class ServerSessionWatch : IDisposable
    {
        public event EventHandler<SessionStatusArgs> SessionChanged;


        public ServerSessionWatch()
        {
            PhotonServer.Instance.Sessions.SessionChanged += Session_OnChanged;
        }

        public void Dispose()
        {
            PhotonServer.Instance.Sessions.SessionChanged -= Session_OnChanged;
        }

        public void Initialize()
        {
            var sessionList = PhotonServer.Instance.Sessions.All
                .OrderBy(x => x.TimeCreated).ToArray();

            foreach (var session in sessionList)
                SendUpdate(session);
        }

        private void SendUpdate(ServerSessionBase session)
        {
            uint? number = null;
            string name = null;
            string projectName = null;
            string projectVersion = null;
            string gitRefspec = null;
            string status;

            if (!session.IsReleased) {
                status = "running";
            }
            else if (session.Exception != null) {
                status = "failed";
            }
            else {
                status = "success";
            }

            if (session is ServerBuildSession buildSession) {
                number = buildSession.Build.Number;
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
                status,
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

        private string GetSessionType(ServerSessionBase session)
        {
            if (session is ServerBuildSession) return "build";
            if (session is ServerDeploySession) return "deploy";
            if (session is ServerUpdateSession) return "update";
            return null;
        }
    }
}
