using Photon.Communication.Messages;

namespace Photon.Library.TcpMessages
{
    public class AgentUpdateRequest : IFileRequestMessage
    {
        public string MessageId {get; set;}
        public string Filename {get; set;}
    }
}
