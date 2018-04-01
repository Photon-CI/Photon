using Photon.Framework.Communication;

namespace Photon.Framework.Messages
{
    public class SessionReleaseRequest : IRequestMessage
    {
        public string MessageId {get; set;}
        public string SessionId {get; set;}
    }
}
