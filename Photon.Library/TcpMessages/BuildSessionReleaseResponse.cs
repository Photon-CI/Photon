using Photon.Communication.Messages;

namespace Photon.Library.TcpMessages
{
    public class BuildSessionReleaseResponse : IResponseMessage
    {
        public string MessageId {get; set;}
        public string RequestMessageId {get; set;}
        public bool Successful {get; set;}
    }
}
