using Photon.Agent.Internal;
using Photon.Communication;
using Photon.Communication.Messages;
using System.Threading.Tasks;
using Photon.Library.TcpMessages;

namespace Photon.Agent.MessageHandlers
{
    public class DeploySessionReleaseProcessor : MessageProcessorBase<DeploySessionReleaseRequest>
    {
        public override async Task<IResponseMessage> Process(DeploySessionReleaseRequest requestMessage)
        {
            await PhotonAgent.Instance.Sessions.ReleaseSessionAsync(requestMessage.AgentSessionId);

            return new DeploySessionReleaseResponse {
                Successful = true,
            };
        }
    }
}
