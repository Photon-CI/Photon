using Photon.Agent.Internal;
using Photon.Communication;
using Photon.Communication.Messages;
using Photon.Library.TcpMessages;
using System.Threading.Tasks;

namespace Photon.Agent.MessageHandlers
{
    public class SessionReleaseProcessor : MessageProcessorBase<SessionReleaseRequest>
    {
        public override Task<IResponseMessage> Process(SessionReleaseRequest requestMessage)
        {
            var _ = Task.Delay(100).ContinueWith(async t => {
                await PhotonAgent.Instance.Sessions.ReleaseSessionAsync(requestMessage.AgentSessionId);
            });

            return Task.FromResult<IResponseMessage>(null);
        }
    }
}
