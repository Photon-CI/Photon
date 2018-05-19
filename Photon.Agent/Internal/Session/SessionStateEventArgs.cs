using System;

namespace Photon.Agent.Internal.Session
{
    internal class SessionStateEventArgs : EventArgs
    {
        public AgentSessionBase Session {get;}

        public SessionStateEventArgs(AgentSessionBase session)
        {
            this.Session = session;
        }
    }
}
