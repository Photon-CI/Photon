using Photon.Communication.Messages;

namespace Photon.Framework.TcpMessages
{
    public class BuildTaskOutputMessage : IRequestMessage
    {
        public string MessageId {get; set;}
        public string TaskSessionId {get; set;}
        public string Text {get; set;}
    }
}
