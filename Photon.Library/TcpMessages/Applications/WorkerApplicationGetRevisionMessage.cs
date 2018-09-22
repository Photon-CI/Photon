using Photon.Communication.Messages;
using Photon.Framework.Applications;

namespace Photon.Library.TcpMessages.Applications
{
    public class WorkerApplicationGetRevisionRequest : IRequestMessage
    {
        public string MessageId {get; set;}
        public string AgentSessionId {get; set;}
        public string ProjectId {get; set;}
        public string ApplicationName {get; set;}
        public uint DeploymentNumber {get; set;}
    }

    public class WorkerApplicationGetRevisionResponse : ResponseMessageBase
    {
        public DomainApplicationRevision Revision {get; set;}
    }
}
