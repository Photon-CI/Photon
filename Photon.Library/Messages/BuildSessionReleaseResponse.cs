using Photon.Communication;

namespace Photon.Library.Messages
{
    public class BuildSessionReleaseResponse : IResponseMessage
    {
        public string MessageId {get; set;}
        public string RequestMessageId {get; set;}
        public bool Successful {get; set;}
    }
}
