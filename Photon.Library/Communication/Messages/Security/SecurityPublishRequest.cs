using Photon.Communication.Messages;
using Photon.Library.Security;

namespace Photon.Library.TcpMessages.Security
{
    public class SecurityPublishRequest : IRequestMessage
    {
        public string MessageId {get; set;}
        public UserGroup[] UserGroups {get; set;}
        public User[] Users {get; set;}
        public bool SecurityEnabled {get; set;}
        public bool SecurityDomainEnabled {get; set;}
    }
}
