using System;
using System.Linq;
using Photon.Library.Session;

namespace Photon.Agent.Internal.Session
{

    internal class AgentSessionWatch : IDisposable
    {
        public event EventHandler<SessionStatusArgs> SessionChanged;


        public AgentSessionWatch()
        {
            PhotonAgent.Instance.Sessions.SessionStarted += OnSessionStartedStopped;
            PhotonAgent.Instance.Sessions.SessionReleased += OnSessionStartedStopped;
        }

        public void Dispose()
        {
            PhotonAgent.Instance.Sessions.SessionStarted -= OnSessionStartedStopped;
            PhotonAgent.Instance.Sessions.SessionReleased -= OnSessionStartedStopped;
        }

        public void Initialize()
        {
            var sessionList = PhotonAgent.Instance.Sessions.All
                .OrderByDescending(x => x.TimeCreated)
                .Take(20).Reverse().ToArray();

            foreach (var session in sessionList)
                SendUpdate(session);
        }

        public void OnSessionStartedStopped(object sender, SessionStateEventArgs e)
        {
            SendUpdate(e.Session);
        }

        private void SendUpdate(AgentSessionBase session)
        {
            uint? number = null;
            string name = null;
            string projectName = null;
            string projectVersion = null;
            string gitRefspec = null;

            if (session is AgentBuildSession buildSession) {
                number = buildSession.BuildNumber;
                name = "?"; // buildSession.TaskName;
                projectName = buildSession.Project?.Name;
                gitRefspec = buildSession.GitRefspec;
            }
            else if (session is AgentDeploySession deploySession) {
                number = deploySession.DeploymentNumber;
                name = "?"; // deploySession.ScriptName;
                projectName = deploySession.Project?.Name;
                projectVersion = $"{deploySession.ProjectPackageId} @{deploySession.ProjectPackageVersion}";
            }
            //else if (session is AgentUpdateSession updateSession) {
            //    projectName = "Update Agents";
            //    projectVersion = updateSession.AgentIds != null
            //        ? string.Join(", ", updateSession.AgentIds)
            //        : "<all>";
            //}

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

        private string GetSessionType(AgentSessionBase session)
        {
            if (session is AgentBuildSession) return "build";
            if (session is AgentDeploySession) return "deploy";
            //if (session is AgentUpdateSession) return "update";
            return null;
        }
    }
}
