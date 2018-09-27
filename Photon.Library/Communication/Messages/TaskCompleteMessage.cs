using Photon.Communication.Messages;

namespace Photon.Library.TcpMessages
{
    public class TaskCompleteMessage : IRequestMessage
    {
        public string MessageId {get; set;}
        public string SessionId {get; set;}
        public string TaskId {get; set;}
        public string Text {get; set;}
        public bool Successful {get; set;}
        public string Exception {get; set;}
    }
}
