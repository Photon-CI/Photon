using Photon.Communication;
using Photon.Communication.Messages;

namespace Photon.Library.Messages
{
    public class BuildTaskRunRequest : IRequestMessage
    {
        public string MessageId {get; set;}
        public string AgentSessionId {get; set;}
        public string TaskSessionId {get; set;}
        public string TaskName {get; set;}
    }
}
