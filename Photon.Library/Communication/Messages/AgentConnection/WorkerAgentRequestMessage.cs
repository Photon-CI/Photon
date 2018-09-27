using Photon.Communication.Messages;
using Photon.Framework.AgentConnection;

namespace Photon.Library.TcpMessages.AgentConnection
{
    public class WorkerAgentRequestRequest : IRequestMessage
    {
        public string MessageId {get; set;}
        public string[] AgentIdList {get; set;}
    }

    public class WorkerAgentRequestResponse : ResponseMessageBase
    {
        public AgentConnectionHandle[] ConnectionHandles {get; set;}
    }
}
