using Photon.Communication.Messages;

namespace Photon.Framework.TcpMessages
{
    public class BuildTaskOutputMessage : IRequestMessage
    {
        public string MessageId {get; set;}
        public string AgentSessionId {get; set;}
        public string TaskId {get; set;}
        public string Text {get; set;}
    }
}
