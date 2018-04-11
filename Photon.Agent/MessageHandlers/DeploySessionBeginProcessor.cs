using Photon.Agent.Internal;
using Photon.Agent.Internal.Session;
using Photon.Communication;
using Photon.Communication.Messages;
using Photon.Framework.TcpMessages;
using System.Threading.Tasks;

namespace Photon.Agent.MessageHandlers
{
    public class DeploySessionBeginProcessor : MessageProcessorBase<DeploySessionBeginRequest>
    {
        public override async Task<IResponseMessage> Process(DeploySessionBeginRequest requestMessage)
        {
            var response = new DeploySessionBeginResponse();

            var session = new AgentDeploySession(Transceiver, requestMessage.ServerSessionId) {
                Project = null, // TODO: Store ProjectId in package
                ProjectPackageId = requestMessage.ProjectPackageId,
                ProjectPackageVersion = requestMessage.ProjectPackageVersion,
            };

            PhotonAgent.Instance.Sessions.BeginSession(session);

            await session.InitializeAsync();

            response.AgentSessionId = session.SessionId;

            return response;
        }
    }
}
