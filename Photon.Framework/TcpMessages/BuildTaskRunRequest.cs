using Photon.Communication.Messages;

namespace Photon.Framework.TcpMessages
{
    public class BuildTaskRunRequest : IRequestMessage
    {
        public string MessageId {get; set;}
        public string AgentSessionId {get; set;}
        public string TaskSessionId {get; set;}
        public string TaskName {get; set;}
    }
}
