using Photon.Agent.Internal;
using Photon.Communication;
using Photon.Communication.Messages;
using Photon.Framework.Messages;
using System.Threading.Tasks;

namespace Photon.Agent.MessageHandlers
{
    public class BuildSessionReleaseProcessor : MessageProcessorBase<BuildSessionReleaseRequest>
    {
        public override async Task<IResponseMessage> Process(BuildSessionReleaseRequest requestMessage)
        {
            await PhotonAgent.Instance.Sessions.ReleaseSessionAsync(requestMessage.SessionId);

            return new BuildSessionReleaseResponse {
                Successful = true,
            };
        }
    }
}
