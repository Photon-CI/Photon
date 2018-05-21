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
                Project = requestMessage.Project,
                ProjectPackageId = requestMessage.ProjectPackageId,
                ProjectPackageVersion = requestMessage.ProjectPackageVersion,
                ServerVariables = requestMessage.Variables,
                Agent = requestMessage.Agent,
            };

            await session.InitializeAsync();

            PhotonAgent.Instance.Sessions.BeginSession(session);

            response.AgentSessionId = session.SessionId;

            return response;
        }
    }
}
