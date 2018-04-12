using Newtonsoft.Json;
using Photon.Communication.Messages;

namespace Photon.Library.TcpMessages
{
    public class ProjectPackagePullResponse : ResponseMessageBase, IFileResponseMessage
    {
        [JsonIgnore]
        public string Filename {get; set;}
    }
}
