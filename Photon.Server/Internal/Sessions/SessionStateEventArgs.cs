using System;

namespace Photon.Server.Internal.Sessions
{
    internal class SessionStateEventArgs : EventArgs
    {
        public ServerSessionBase Session {get;}

        public SessionStateEventArgs(ServerSessionBase session)
        {
            this.Session = session;
        }
    }
}
