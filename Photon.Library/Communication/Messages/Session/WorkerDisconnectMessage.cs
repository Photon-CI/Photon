using Photon.Communication.Messages;

namespace Photon.Library.TcpMessages.Worker
{
    public class WorkerDisconnectRequestMessage : IRequestMessage
    {
        public string MessageId {get; set;}
    }

    public class WorkerDisconnectResponseMessage : ResponseMessageBase
    {
        //
    }
}
