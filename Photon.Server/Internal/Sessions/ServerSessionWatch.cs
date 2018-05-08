using System;

namespace Photon.Server.Internal.Sessions
{
    internal class SessionStatusArgs : EventArgs
    {
        public object Data {get; set;}

        public SessionStatusArgs(object data)
        {
            this.Data = data;
        }
    }

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
            var data = new {
                id = session.SessionId,
                type = GetSessionType(session),
                isReleased = session.IsReleased,
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
