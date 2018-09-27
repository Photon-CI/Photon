using Photon.Communication.Messages;

namespace Photon.Library.TcpMessages
{
    public class SessionOutputMessage : IRequestMessage
    {
        public string MessageId {get; set;}
        public string ServerSessionId {get; set;}
        public string AgentSessionId {get; set;}
        //public string SessionClientId {get; set;}
        public string Text {get; set;}
    }
}
