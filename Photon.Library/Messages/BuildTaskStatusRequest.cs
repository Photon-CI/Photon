using Photon.Communication;

namespace Photon.Library.Messages
{
    public class BuildTaskStatusRequest : IRequestMessage
    {
        public string MessageId {get; set;}
        public string SessionId {get; set;}
        public string TaskId {get; set;}
        public int OutputStart {get; set;}
    }
}
