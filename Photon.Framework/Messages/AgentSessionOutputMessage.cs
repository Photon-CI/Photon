using Photon.Communication;
using Photon.Communication.Messages;

namespace Photon.Framework.Messages
{
    public class AgentSessionOutputMessage : IRequestMessage
    {
        public string MessageId {get; set;}
        public string ServerSessionId {get; set;}
        public string Text {get; set;}
    }
}
