using Photon.Communication.Messages;

namespace Photon.Library.TcpMessages.Session
{
    public class WorkerBuildSessionRunRequest : IRequestMessage
    {
        public string MessageId {get; set;}
    }

    //public class WorkerBuildSessionRunResponse : ResponseMessageBase
    //{
    //    //
    //}
}
