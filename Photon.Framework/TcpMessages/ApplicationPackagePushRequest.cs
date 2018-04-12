using Newtonsoft.Json;
using Photon.Communication.Messages;

namespace Photon.Framework.TcpMessages
{
    public class ApplicationPackagePushRequest : IFileRequestMessage
    {
        public string MessageId {get; set;}

        [JsonIgnore]
        public string Filename {get; set;}
    }
}
