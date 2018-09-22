using Photon.Communication;
using Photon.Framework.Applications;
using Photon.Library.TcpMessages.Applications;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Worker.Internal.Clients
{
    internal class ApplicationClient : IApplicationWriter
    {
        private readonly MessageTransceiver transceiver;

        public string AgentSessionId {get; set;}
        public string ProjectId {get; set;}
        public uint DeploymentNumber {get; set;}


        public ApplicationClient(MessageTransceiver transceiver)
        {
            this.transceiver = transceiver;
        }

        public async Task<DomainApplicationRevision> GetRevision(string appName, CancellationToken token = default)
        {
            var request = new WorkerApplicationGetRevisionRequest {
                AgentSessionId = AgentSessionId,
                ProjectId = ProjectId,
                ApplicationName = appName,
                DeploymentNumber = DeploymentNumber,
            };

            var response = await transceiver.Send(request)
                .GetResponseAsync<WorkerApplicationGetRevisionResponse>(token);

            return response.Revision;
        }

        public async Task<DomainApplicationRevision> GetRevision(string projectId, string appName, uint deploymentNumber, CancellationToken token = default)
        {
            var request = new WorkerApplicationGetRevisionRequest {
                AgentSessionId = AgentSessionId,
                ProjectId = projectId,
                ApplicationName = appName,
                DeploymentNumber = deploymentNumber,
            };

            var response = await transceiver.Send(request)
                .GetResponseAsync<WorkerApplicationGetRevisionResponse>(token);

            return response.Revision;
        }

        public async Task<DomainApplicationRevision> RegisterRevision(string appName, string packageId, string packageVersion, string environmentName = null, CancellationToken token = default)
        {
            var request = new WorkerApplicationRegisterRevisionRequest {
                AgentSessionId = AgentSessionId,
                Request = {
                    ProjectId = ProjectId,
                    ApplicationName = appName,
                    DeploymentNumber = DeploymentNumber,
                    PackageId = packageId,
                    PackageVersion = packageVersion,
                    EnvironmentName = environmentName,
                },
            };

            var response = await transceiver.Send(request)
                .GetResponseAsync<WorkerApplicationRegisterRevisionResponse>(token);

            return response.Revision;
        }

        public async Task<DomainApplicationRevision> RegisterRevision(DomainApplicationRevisionRequest revisionRequest, CancellationToken token = default)
        {
            var request = new WorkerApplicationRegisterRevisionRequest {
                AgentSessionId = AgentSessionId,
                Request = revisionRequest,
            };

            var response = await transceiver.Send(request)
                .GetResponseAsync<WorkerApplicationRegisterRevisionResponse>(token);

            return response.Revision;
        }
    }
}
