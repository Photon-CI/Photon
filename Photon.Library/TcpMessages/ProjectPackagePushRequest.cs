using Newtonsoft.Json;
using Photon.Communication.Messages;

namespace Photon.Library.TcpMessages
{
    public class ProjectPackagePushRequest : IFileRequestMessage
    {
        public string MessageId {get; set;}

        [JsonIgnore]
        public string Filename {get; set;}
    }
}
