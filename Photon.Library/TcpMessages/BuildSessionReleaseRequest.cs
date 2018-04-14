using Photon.Communication.Messages;

namespace Photon.Library.TcpMessages
{
    public class BuildSessionReleaseRequest : IRequestMessage
    {
        public string MessageId {get; set;}
        public string AgentSessionId {get; set;}
    }
}
