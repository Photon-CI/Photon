using Photon.Communication;

namespace Photon.Framework.Messages
{
    public class BuildTaskOutputMessage : IRequestMessage
    {
        public string MessageId {get; set;}
        public string TaskSessionId {get; set;}
        public string Text {get; set;}
    }
}
