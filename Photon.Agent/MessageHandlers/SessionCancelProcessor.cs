using log4net;
using Photon.Agent.Internal;
using Photon.Communication;
using Photon.Communication.Messages;
using Photon.Library.TcpMessages;
using System.Threading.Tasks;

namespace Photon.Agent.MessageHandlers
{
    public class SessionCancelProcessor : MessageProcessorBase<SessionCancelRequest>
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SessionCancelProcessor));


        public override async Task<IResponseMessage> Process(SessionCancelRequest requestMessage)
        {
            if (PhotonAgent.Instance.Sessions.TryGet(requestMessage.AgentSessionId, out var session)) {
                session.Cancel();
            }
            else {
                Log.Error($"Failed to cancel session '{requestMessage.AgentSessionId}'. Session not found!");
            }

            return null;
        }
    }
}
