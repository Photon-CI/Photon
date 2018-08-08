using Photon.Framework.Pooling;

namespace Photon.Library.HttpSecurity
{
    public class HttpUserContext : LifespanReferenceItem
    {
        public string UserId {get; set;}
        public string Username {get; set;}
    }
}
