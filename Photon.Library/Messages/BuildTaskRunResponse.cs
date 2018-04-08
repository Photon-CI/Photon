using Photon.Communication;
using Photon.Communication.Messages;
using Photon.Framework.Tasks;

namespace Photon.Library.Messages
{
    public class BuildTaskRunResponse : IResponseMessage
    {
        public string MessageId {get; set;}
        public string RequestMessageId {get; set;}
        public TaskResult Result {get; set;}
        public bool Successful {get; set;}
        //public string Output {get; set;}
        public string Exception {get; set;}
    }
}
