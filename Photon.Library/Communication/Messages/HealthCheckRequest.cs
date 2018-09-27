using Photon.Communication.Messages;

namespace Photon.Library.TcpMessages
{
    public class HealthCheckRequest : IRequestMessage
    {
        public string MessageId {get; set;}
    }
}
