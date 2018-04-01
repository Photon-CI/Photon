using Photon.Agent.Internal;
using Photon.Framework.Communication;
using Photon.Framework.Messages;
using System.Threading.Tasks;

namespace Photon.Agent.MessageHandlers
{
    public class SessionBeginProcessor : IProcessMessage<SessionBeginRequest>
    {
        public async Task<IResponseMessage> Process(SessionBeginRequest requestMessage)
        {
            var session = new AgentSession();

            PhotonAgent.Instance.Sessions.BeginSession(session);

            return new SessionBeginResponse {
                SessionId = session.Id,
            };
        }
    }
}
