using Photon.Communication.Messages;

namespace Photon.Library.TcpMessages.Packages
{
    public class WorkerProjectPackagePullRequest : IRequestMessage
    {
        public string MessageId {get; set;}
        public string AgentSessionId {get; set;}
        public string PackageId {get; set;}
        public string PackageVersion {get; set;}
    }

    public class WorkerProjectPackagePullResponse : ResponseMessageBase
    {
        public string Filename {get; set;}
    }
}
