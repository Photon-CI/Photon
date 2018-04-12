using Photon.Framework.Tasks;
using System;
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

        public async Task BeginAsync()
        {
            await RemoteTaskCompletionSource<object>.Run((task, sponsor) => {
                Client.Begin(task);
            });
        }

        public async Task ReleaseAsync()
        {
            await RemoteTaskCompletionSource<object>.Run((task, sponsor) => {
                Client.ReleaseAsync(task);
            });
        }

        public async Task<TaskResult> RunTaskAsync(string taskName)
        {
            return await RemoteTaskCompletionSource<TaskResult>.Run((task, sponsor) => {
                Client.RunTaskAsync(taskName, task);
            });
        }
    }
}
