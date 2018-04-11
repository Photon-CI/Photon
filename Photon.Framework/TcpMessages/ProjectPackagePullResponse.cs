using Newtonsoft.Json;
using Photon.Communication.Messages;

namespace Photon.Framework.TcpMessages
{
    public class ProjectPackagePullResponse : ResponseMessageBase, IFileResponseMessage
    {
        [JsonIgnore]
        public string Filename {get; set;}
    }
}
