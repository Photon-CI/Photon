using Newtonsoft.Json;
using Photon.Communication.Messages;

namespace Photon.Library.TcpMessages.Packages
{
    public class AgentProjectPackagePullRequest : IRequestMessage
    {
        public string MessageId {get; set;}
        public string ProjectPackageId {get; set;}
        public string ProjectPackageVersion {get; set;}
    }

    public class AgentProjectPackagePullResponse : ResponseMessageBase, IFileResponseMessage
    {
        [JsonIgnore]
        public string Filename {get; set;}
    }
}
