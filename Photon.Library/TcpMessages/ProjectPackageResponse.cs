using Photon.Communication.Messages;

namespace Photon.Library.TcpMessages
{
    public class ProjectPackageResponse : IResponseMessage
    {
        public string MessageId {get; set;}
        public string RequestMessageId {get; set;}
        public bool Successful {get; set;}
        public string Exception {get; set;}
    }
}
