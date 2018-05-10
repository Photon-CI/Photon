using Photon.Communication.Messages;
using Photon.Framework.Variables;

namespace Photon.Library.TcpMessages
{
    public class DeploySessionBeginRequest : IRequestMessage
    {
        public string MessageId {get; set;}
        public uint DeploymentNumber {get; set;}
        public string ServerSessionId {get; set;}
        public string SessionClientId {get; set;}
        public string ProjectPackageId {get; set;}
        public string ProjectPackageVersion {get; set;}
        public VariableSetCollection Variables {get; set;}
        public string EnvironmentName {get; set;}
    }
}
