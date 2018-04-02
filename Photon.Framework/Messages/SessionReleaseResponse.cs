using Photon.Communication;

namespace Photon.Framework.Messages
{
    public class SessionReleaseResponse : IResponseMessage
    {
        public string MessageId {get; set;}
        public string RequestMessageId {get; set;}
        public bool Successful {get; set;}
    }
}
