using Photon.Communication.Messages;

namespace Photon.Library.TcpMessages.Session
{
    public class WorkerDeploymentSessionRunRequest : IRequestMessage
    {
        public string MessageId {get; set;}
    }

    //public class WorkerDeploymentSessionRunResponse : ResponseMessageBase
    //{
    //    //
    //}
}
