using Photon.Agent.Internal;
using Photon.Agent.Internal.Session;
using Photon.Communication;
using Photon.Communication.Messages;
using Photon.Library.TcpMessages;
using System.Threading.Tasks;

namespace Photon.Agent.MessageHandlers
{
    public class DeploySessionBeginProcessor : MessageProcessorBase<DeploySessionBeginRequest>
    {
        public override async Task<IResponseMessage> Process(DeploySessionBeginRequest requestMessage)
        {
            var response = new DeploySessionBeginResponse();

            var session = new AgentDeploySession(Transceiver, requestMessage.ServerSessionId, requestMessage.SessionClientId) {
                DeploymentNumber = requestMessage.DeploymentNumber,
                EnvironmentName = requestMessage.EnvironmentName,
                Project = null, // TODO: Store ProjectId in package
                ProjectPackageId = requestMessage.ProjectPackageId,
                ProjectPackageVersion = requestMessage.ProjectPackageVersion,
                ServerVariables = requestMessage.Variables,
            };

            PhotonAgent.Instance.Sessions.BeginSession(session);

            await session.InitializeAsync();

            response.AgentSessionId = session.SessionId;

            return response;
        }
    }
}
