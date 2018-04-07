using Photon.Communication;

namespace Photon.Library.Messages
{
    public class BuildTaskStartResponse : IResponseMessage
    {
        public string MessageId {get; set;}
        public string RequestMessageId {get; set;}
        public string TaskId {get; set;}
        public bool Successful {get; set;}
        public string Exception {get; set;}
    }
}
