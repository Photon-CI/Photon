using Photon.Communication.Messages;

namespace Photon.Library.TcpMessages.Status
{
    public class WorkerStatusGetRequest : IRequestMessage
    {
        public string MessageId {get; set;}
    }

    public class WorkerStatusGetResponse : ResponseMessageBase
    {
        public string HostName {get; set;}
    }
}
