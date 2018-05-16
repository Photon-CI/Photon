using Photon.Framework.Server;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Framework.Domain
{
    public class DomainAgentSessionHandle : IDisposable
    {
        public ServerAgent Agent {get;}
        public DomainAgentSessionClient Client {get; private set;}


        public DomainAgentSessionHandle(ServerAgent agent, DomainAgentSessionClient client)
        {
            this.Agent = agent;
            this.Client = client;
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

        public async Task RunTaskAsync(string taskName, CancellationToken token = default(CancellationToken))
        {
            await RemoteTaskCompletionSource.Run((task, sponsor) => {
                Client.RunTaskAsync(taskName, task);
            }, token);
        }

        public async Task<string[]> GetTaskRolesAsync(string taskName, CancellationToken token = default(CancellationToken))
        {
            return await RemoteTaskCompletionSource<string[]>.Run((task, sponsor) => {
                Client.GetTaskRolesAsync(taskName, task);
            }, token);
        }
    }
}
