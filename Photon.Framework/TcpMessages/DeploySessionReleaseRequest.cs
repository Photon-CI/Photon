using Photon.Communication.Messages;

namespace Photon.Framework.TcpMessages
{
    public class DeploySessionReleaseRequest : IRequestMessage
    {
        public string MessageId {get; set;}
        public string SessionId {get; set;}
    }
}
