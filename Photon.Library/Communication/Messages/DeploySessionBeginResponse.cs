using Photon.Communication.Messages;

namespace Photon.Library.TcpMessages
{
    public class DeploySessionBeginResponse : ResponseMessageBase
    {
        public string AgentSessionId {get; set;}
    }
}
