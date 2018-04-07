using Photon.Communication;

namespace Photon.Framework.Messages
{
    public class BuildSessionBeginResponse : IResponseMessage
    {
        public string MessageId {get; set;}
        public string RequestMessageId {get; set;}
        public bool Successful {get; set;}
        public string Exception {get; set;}
        public string SessionId {get; set;}
    }
}
