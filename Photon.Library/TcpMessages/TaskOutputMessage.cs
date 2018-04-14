using Photon.Communication.Messages;

namespace Photon.Library.TcpMessages
{
    public class TaskOutputMessage : IRequestMessage
    {
        public string MessageId {get; set;}
        public string ServerSessionId {get; set;}
        public string SessionClientId {get; set;}
        public string TaskId {get; set;}
        public string Text {get; set;}
    }
}
