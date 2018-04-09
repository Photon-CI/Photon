using Photon.Communication.Messages;

namespace Photon.Framework.TcpMessages
{
    public class DeploySessionReleaseResponse : IResponseMessage
    {
        public string MessageId {get; set;}
        public string RequestMessageId {get; set;}
        public bool Successful {get; set;}
    }
}
