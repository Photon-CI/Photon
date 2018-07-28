using Newtonsoft.Json;
using Photon.Communication.Messages;

namespace Photon.Library.TcpMessages
{
    public class ApplicationPackagePushRequest : IFileRequestMessage
    {
        public string MessageId {get; set;}

        public string ServerSessionId {get; set;}

        [JsonIgnore]
        public string Filename {get; set;}
    }
}
