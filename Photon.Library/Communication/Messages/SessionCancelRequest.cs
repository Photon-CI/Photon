using Photon.Communication.Messages;

namespace Photon.Library.TcpMessages
{
    public class SessionCancelRequest : IRequestMessage
    {
        public string MessageId {get; set;}
        public string AgentSessionId {get; set;}
    }
}
