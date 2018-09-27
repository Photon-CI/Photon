using Photon.Communication.Messages;

namespace Photon.Library.TcpMessages
{
    public class AgentGetVersionResponse : ResponseMessageBase
    {
        public string Version {get; set;}
    }
}
