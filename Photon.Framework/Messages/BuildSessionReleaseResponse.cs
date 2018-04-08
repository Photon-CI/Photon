using Photon.Communication;
using Photon.Communication.Messages;

namespace Photon.Framework.Messages
{
    public class BuildSessionReleaseResponse : IResponseMessage
    {
        public string MessageId {get; set;}
        public string RequestMessageId {get; set;}
        public bool Successful {get; set;}
    }
}
