using Photon.Communication;
using Photon.Framework.AgentConnection;
using Photon.Library.TcpMessages.AgentConnection;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Worker.Internal
{
    internal class WorkerAgentConnection : IAgentConnection
    {
        public string AgentId {get; set;}
        public string ConnectionId {get; set;}
        public MessageTransceiver Transceiver {get; set;}


        public WorkerAgentConnection() {}

        public void Dispose() {}

        public async Task BeginAsync(CancellationToken token)
        {
            var request = new ServerAgentConnectionBeginRequest {
                // TODO
            };

            var response = await Transceiver.Send(request)
                .GetResponseAsync<ServerAgentConnectionBeginResponse>(token);

            // TODO
        }

        public async Task ReleaseAsync(CancellationToken token)
        {
            var request = new ServerAgentConnectionReleaseRequest {
                // TODO
            };

            var response = await Transceiver.Send(request)
                .GetResponseAsync<ServerAgentConnectionReleaseResponse>(token);

            // TODO
        }

        public async Task RunTaskAsync(string taskName, CancellationToken token = default)
        {
            var request = new ServerAgentConnectionRunRequest {
                // TODO
            };

            var response = await Transceiver.Send(request)
                .GetResponseAsync<ServerAgentConnectionRunResponse>(token);

            // TODO
        }
    }
}
