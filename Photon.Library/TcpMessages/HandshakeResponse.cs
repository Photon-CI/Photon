using Photon.Communication.Messages;

namespace Photon.Library.TcpMessages
{
    public class HandshakeResponse : ResponseMessageBase
    {
        public string Key {get; set;}
        public string AgentVersion {get; set;}
        public bool PasswordMatch {get; set;}
    }
}
