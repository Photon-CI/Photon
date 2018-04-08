using Newtonsoft.Json;
using Photon.Communication.Messages;

namespace Photon.Library.Messages
{
    public class ProjectPackageRequest : IFileRequestMessage
    {
        public string MessageId {get; set;}

        [JsonIgnore]
        public string Filename {get; set;}
    }
}
