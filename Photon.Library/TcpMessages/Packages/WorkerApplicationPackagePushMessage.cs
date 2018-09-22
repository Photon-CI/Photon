using Photon.Communication.Messages;

namespace Photon.Library.TcpMessages.Packages
{
    public class WorkerApplicationPackagePushRequest : IRequestMessage
    {
        public string MessageId {get; set;}
        public string AgentSessionId {get; set;}
        public string Filename {get; set;}
    }

    //public class WorkerApplicationPackagePushResponse : ResponseMessageBase
    //{
    //    //
    //}
}
