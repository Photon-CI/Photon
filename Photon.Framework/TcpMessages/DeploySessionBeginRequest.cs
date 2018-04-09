using Photon.Communication.Messages;

namespace Photon.Framework.TcpMessages
{
    public class DeploySessionBeginRequest : IRequestMessage
    {
        public string MessageId {get; set;}
        public string ServerSessionId {get; set;}
        public string ProjectPackageId {get; set;}
        public string ProjectPackageVersion {get; set;}
    }
}
