using Photon.Communication;

namespace Photon.Library.Messages
{
    public class BuildSessionReleaseRequest : IRequestMessage
    {
        public string MessageId {get; set;}
        public string SessionId {get; set;}
    }
}
