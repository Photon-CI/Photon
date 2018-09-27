using Photon.Communication.Messages;

namespace Photon.Library.TcpMessages.AgentConnection
{
    public class ServerAgentConnectionBeginRequest : IRequestMessage
    {
        public string MessageId {get; set;}
    }

    public class ServerAgentConnectionBeginResponse : ResponseMessageBase
    {
        //
    }
}
