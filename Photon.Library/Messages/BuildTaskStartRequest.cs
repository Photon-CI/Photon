using Photon.Communication;

namespace Photon.Library.Messages
{
    public class BuildTaskStartRequest : IRequestMessage
    {
        public string MessageId {get; set;}
        public string SessionId {get; set;}
        public string TaskName {get; set;}
    }
}
