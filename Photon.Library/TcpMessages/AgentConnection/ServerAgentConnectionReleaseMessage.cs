using Photon.Communication.Messages;

namespace Photon.Library.TcpMessages.AgentConnection
{
    public class ServerAgentConnectionReleaseRequest : IRequestMessage
    {
        public string MessageId {get; set;}
    }

    public class ServerAgentConnectionReleaseResponse : ResponseMessageBase
    {
        //
    }
}
