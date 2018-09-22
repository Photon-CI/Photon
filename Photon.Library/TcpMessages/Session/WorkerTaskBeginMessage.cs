using Photon.Communication.Messages;

namespace Photon.Library.TcpMessages.Worker
{
    public class WorkerTaskBeginRequestMessage : IRequestMessage
    {
        public string MessageId {get; set;}
        public string TaskName {get; set;}
    }

    public class WorkerTaskBeginResponseMessage : ResponseMessageBase
    {
        //
    }
}
