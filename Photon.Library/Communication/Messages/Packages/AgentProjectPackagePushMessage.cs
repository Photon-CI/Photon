using Newtonsoft.Json;
using Photon.Communication.Messages;

namespace Photon.Library.TcpMessages.Packages
{
    public class AgentProjectPackagePushRequest : IFileRequestMessage
    {
        public string MessageId {get; set;}
        public string ServerSessionId {get; set;}

        [JsonIgnore]
        public string Filename {get; set;}
    }

    //public class AgentProjectPackagePushResponse : ResponseMessageBase
    //{
    //    //
    //}
}
