using Photon.Communication;

namespace Photon.Framework.Messages
{
    public class SessionBeginResponse : IResponseMessage
    {
        public string MessageId {get; set;}
        public string RequestMessageId {get; set;}
        public string SessionId {get; set;}
    }
}
