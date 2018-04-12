using Photon.Communication.Messages;

namespace Photon.Framework.TcpMessages
{
    public class DeploySessionBeginResponse : ResponseMessageBase
    {
        public string AgentSessionId {get; set;}
    }
}
