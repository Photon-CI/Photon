using System;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Server.Internal.AgentConnections
{
    internal class AgentConnection : IDisposable
    {
        public event EventHandler<AgentConnectionReleaseEventArgs> Released;

        private bool isReleased;

        public string AgentId {get;}


        public AgentConnection(string agentId)
        {
            this.AgentId = agentId;
        }

        public void Dispose()
        {
            if (!isReleased) {
                OnReleased();
                isReleased = true;
            }

            //...
        }

        public async Task BeginAsync(CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public async Task ReleaseAsync(CancellationToken token = default)
        {
            if (isReleased) return;
            isReleased = true;

            OnReleased();
        }

        public async Task RunTaskAsync(string taskName, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        protected virtual void OnReleased()
        {
            var e = new AgentConnectionReleaseEventArgs(AgentId);
            Released?.Invoke(this, e);
        }
    }

    internal class AgentConnectionReleaseEventArgs : EventArgs
    {
        public string AgentId {get;}


        public AgentConnectionReleaseEventArgs(string agentId)
        {
            this.AgentId = agentId;
        }
    }
}
