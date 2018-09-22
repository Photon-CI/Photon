using Photon.Communication;
using Photon.Framework.Packages;
using Photon.Library.TcpMessages.Packages;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Worker.Internal.Clients
{
    internal class PackageClient : IPackageClient
    {
        private readonly string agentSessionId;
        private readonly MessageTransceiver transceiver;


        public PackageClient(MessageTransceiver transceiver, string agentSessionId)
        {
            this.transceiver = transceiver;
            this.agentSessionId = agentSessionId;
        }

        public async Task PushProjectPackageAsync(string filename, CancellationToken token = default)
        {
            var request = new WorkerProjectPackagePushRequest {
                AgentSessionId = agentSessionId,
                Filename = filename,
            };

            await transceiver.Send(request).GetResponseAsync(token);
        }

        public async Task<string> PullProjectPackageAsync(string id, string version, CancellationToken token = default)
        {
            var request = new WorkerProjectPackagePullRequest {
                AgentSessionId = agentSessionId,
                PackageId = id,
                PackageVersion = version,
            };

            var response = await transceiver.Send(request)
                .GetResponseAsync<WorkerProjectPackagePullResponse>(token);

            return response.Filename;
        }

        public async Task PushApplicationPackageAsync(string filename, CancellationToken token = default(CancellationToken))
        {
            var request = new WorkerApplicationPackagePushRequest {
                AgentSessionId = agentSessionId,
                Filename = filename,
            };

            await transceiver.Send(request).GetResponseAsync(token);
        }

        public async Task<string> PullApplicationPackageAsync(string id, string version, CancellationToken token = default)
        {
            var request = new WorkerApplicationPackagePullRequest {
                AgentSessionId = agentSessionId,
                PackageId = id,
                PackageVersion = version,
            };

            var response = await transceiver.Send(request)
                .GetResponseAsync<WorkerApplicationPackagePullResponse>(token);

            return response.Filename;
        }
    }
}
