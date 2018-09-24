using Photon.Communication.Messages;

namespace Photon.Library.TcpMessages.AgentConnection
{
    public class ServerAgentConnectionRunRequest : IRequestMessage
    {
        public string MessageId {get; set;}
    }

    public class ServerAgentConnectionRunResponse : ResponseMessageBase
    {
        //
    }
}
