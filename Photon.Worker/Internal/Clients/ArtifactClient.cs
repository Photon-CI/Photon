using Photon.Communication;
using Photon.Framework.Artifacts;
using Photon.Library.TcpMessages.Artifacts;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Worker.Internal.Clients
{
    internal class ArtifactClient : IArtifactBuildClient, IArtifactDeploymentClient
    {
        private readonly MessageTransceiver transceiver;

        public string ProjectId {get; set;}


        public ArtifactClient(MessageTransceiver transceiver)
        {
            this.transceiver = transceiver;
        }

        public async Task<string> GetBuildArtifactAsync(uint buildNumber, string filename, CancellationToken token = default)
        {
            var request = new WorkerBuildArtifactGetRequest {
                ProjectId = ProjectId,
                BuildNumber = buildNumber,
                Filename = filename,
            };

            var response = await transceiver.Send(request)
                .GetResponseAsync<WorkerBuildArtifactGetResponse>(token);

            return response.Filename;
        }

        public async Task<string> GetDeploymentArtifactAsync(uint deploymentNumber, string filename, CancellationToken token = default)
        {
            var request = new WorkerDeploymentArtifactGetRequest {
                ProjectId = ProjectId,
                DeploymentNumber = deploymentNumber,
                Filename = filename,
            };

            var response = await transceiver.Send(request)
                .GetResponseAsync<WorkerDeploymentArtifactGetResponse>(token);

            return response.Filename;
        }

        public async Task ArchiveBuildArtifactAsync(uint buildNumber, string filename, CancellationToken token = default)
        {
            var request = new WorkerBuildArtifactArchiveRequest {
                ProjectId = ProjectId,
                BuildNumber = buildNumber,
                Filename = filename,
            };

            await transceiver.Send(request).GetResponseAsync(token);
        }

        public async Task ArchiveDeploymentArtifactAsync(uint deploymentNumber, string filename, CancellationToken token = default)
        {
            var request = new WorkerDeploymentArtifactArchiveRequest {
                ProjectId = ProjectId,
                DeploymentNumber = deploymentNumber,
                Filename = filename,
            };

            await transceiver.Send(request).GetResponseAsync(token);
        }
    }
}
