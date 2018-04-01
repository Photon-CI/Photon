using Photon.Framework.Communication;

namespace Photon.Framework.Messages
{
    public class TaskBeginResponse : IResponseMessage
    {
        public string MessageId {get; set;}
        public string RequestMessageId {get; set;}
        public string TaskId {get; set;}
    }
}
