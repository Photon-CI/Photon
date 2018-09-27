using Photon.Communication.Messages;

namespace Photon.Library.TcpMessages
{
    public class HandshakeRequest : IRequestMessage
    {
        public string MessageId {get; set;}
        public string Key {get; set;}
        public string ServerVersion {get; set;}
        public bool Password {get; set;}
    }
}
