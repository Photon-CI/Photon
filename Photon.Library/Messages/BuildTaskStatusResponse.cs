using Photon.Communication;

namespace Photon.Library.Messages
{
    public class BuildTaskStatusResponse : IResponseMessage
    {
        public string MessageId {get; set;}
        public string RequestMessageId {get; set;}

        public string OutputText {get; set;}
        public int OutputPosition {get; set;}
        public bool Complete {get; set;}
        public bool Successful {get; set;}
        public string Exception {get; set;}
    }
}
