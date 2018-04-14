using Newtonsoft.Json;
using Photon.Communication.Messages;

namespace Photon.Library.TcpMessages
{
    public class ApplicationPackagePullResponse : ResponseMessageBase, IFileResponseMessage
    {
        [JsonIgnore]
        public string Filename {get; set;}
    }
}
