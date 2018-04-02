using Photon.Communication;

namespace Photon.Framework.Messages
{
    public class TaskBeginRequest : IRequestMessage
    {
        public string MessageId {get; set;}
        public string SessionId {get; set;}
        public string TaskName {get; set;}
    }
}
