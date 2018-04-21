using Photon.Communication.Messages;

namespace Photon.Library.TcpMessages
{
    public class AgentGetVersionRequest : IRequestMessage
    {
        public string MessageId {get; set;}
    }
}
