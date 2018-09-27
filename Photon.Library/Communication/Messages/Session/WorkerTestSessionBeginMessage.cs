using Photon.Communication.Messages;

namespace Photon.Library.TcpMessages.Session
{
    public class WorkerTestSessionBeginRequest : IRequestMessage
    {
        public string MessageId {get; set;}
    }

    //public class WorkerTestSessionBeginResponse : ResponseMessageBase
    //{
    //    public string ServerSessionId {get; set;}
    //}
}
