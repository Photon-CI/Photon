using Photon.Agent.Internal;
using Photon.Communication;
using Photon.Library.Messages;
using System.Threading.Tasks;
using Photon.Framework.Messages;

namespace Photon.Agent.MessageHandlers
{
    public class BuildSessionReleaseProcessor : IProcessMessage<BuildSessionReleaseRequest>
    {
        public async Task<IResponseMessage> Process(BuildSessionReleaseRequest requestMessage)
        {
            await PhotonAgent.Instance.Sessions.ReleaseSessionAsync(requestMessage.SessionId);

            return new BuildSessionReleaseResponse {
                Successful = true,
            };
        }
    }
}
