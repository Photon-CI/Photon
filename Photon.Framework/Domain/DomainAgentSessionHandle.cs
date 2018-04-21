using System;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Framework.Domain
{
    public class DomainAgentSessionHandle : IDisposable
    {
        public DomainAgentSessionClient Client {get; private set;}
        //public string AgentSessionId {get;}


        public DomainAgentSessionHandle(DomainAgentSessionClient client/*, string agentSessionId*/)
        {
            this.Client = client;
            //this.AgentSessionId = agentSessionId;
        }

        public void Dispose()
        {
            Client?.Dispose();
            Client = null;
        }

        public async Task BeginAsync(CancellationToken token)
        {
            await RemoteTaskCompletionSource.Run((task, sponsor) => {
                Client.Begin(task);
            }, token);
        }

        public async Task ReleaseAsync(CancellationToken token)
        {
            await RemoteTaskCompletionSource.Run((task, sponsor) => {
                Client.ReleaseAsync(task);
            }, token);
        }

        public async Task RunTaskAsync(string taskName, CancellationToken token)
        {
            await RemoteTaskCompletionSource.Run((task, sponsor) => {
                Client.RunTaskAsync(taskName, task);
            }, token);
        }
    }
}
