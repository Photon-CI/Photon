using Photon.Communication.Messages;
using Photon.Framework.Applications;

namespace Photon.Library.TcpMessages.Applications
{
    public class WorkerApplicationRegisterRevisionRequest : IRequestMessage
    {
        public string MessageId {get; set;}
        public string AgentSessionId {get; set;}
        public DomainApplicationRevisionRequest Request {get; set;}
    }

    public class WorkerApplicationRegisterRevisionResponse : ResponseMessageBase
    {
        public DomainApplicationRevision Revision {get; set;}
    }
}
