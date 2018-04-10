using Newtonsoft.Json;
using Photon.Communication.Messages;

namespace Photon.Framework.TcpMessages
{
    public class ProjectPackagePullResponse : IFileResponseMessage
    {
        public string MessageId {get; set;}
        public string RequestMessageId {get; set;}
        public bool Successful {get; set;}
        public string Exception {get; set;}

        [JsonIgnore]
        public string Filename {get; set;}
    }
}
