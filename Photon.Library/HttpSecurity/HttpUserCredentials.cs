using PiServerLite.Http.Security;

namespace Photon.Library.HttpSecurity
{
    public class HttpUserCredentials : ISecurityUser
    {
        public string Username {get; set;}
        public string Password {get; set;}
    }
}
