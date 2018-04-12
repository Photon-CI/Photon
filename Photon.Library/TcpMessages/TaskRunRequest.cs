using Photon.Communication.Messages;

namespace Photon.Library.TcpMessages
{
    public class TaskRunRequest : IRequestMessage
    {
        public string MessageId {get; set;}
        public string AgentSessionId {get; set;}
        public string TaskSessionId {get; set;}
        public string TaskName {get; set;}
    }
}
