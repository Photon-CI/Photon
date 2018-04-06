using Photon.Communication;

namespace Photon.Framework.Messages
{
    public class BuildTaskRunResponse : IResponseMessage
    {
        public string MessageId {get; set;}
        public string RequestMessageId {get; set;}
        public bool Successful {get; set;}
        public string Output {get; set;}
        public string Exception {get; set;}
    }
}
