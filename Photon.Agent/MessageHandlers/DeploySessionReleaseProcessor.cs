using Photon.Agent.Internal;
using Photon.Communication;
using Photon.Communication.Messages;
using Photon.Framework.TcpMessages;
using System.Threading.Tasks;

namespace Photon.Agent.MessageHandlers
{
    public class DeploySessionReleaseProcessor : MessageProcessorBase<DeploySessionReleaseRequest>
    {
        public override async Task<IResponseMessage> Process(DeploySessionReleaseRequest requestMessage)
        {
            await PhotonAgent.Instance.Sessions.ReleaseSessionAsync(requestMessage.SessionId);

            return new DeploySessionReleaseResponse {
                Successful = true,
            };
        }
    }
}
