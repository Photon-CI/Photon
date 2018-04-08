using Newtonsoft.Json;
using Photon.Communication.Messages;
using System;
using System.IO;

namespace Photon.Library.Messages
{
    public class ProjectPackageRequest : IStreamRequestMessage
    {
        public string MessageId {get; set;}
        public string SessionId {get; set;}

        [JsonIgnore]
        public Func<Stream> StreamFunc {get; set;}
    }
}
