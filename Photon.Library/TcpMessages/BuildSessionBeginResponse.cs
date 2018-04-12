using Photon.Communication.Messages;

namespace Photon.Library.TcpMessages
{
    public class BuildSessionBeginResponse : ResponseMessageBase
    {
        public string SessionId {get; set;}
    }
}
