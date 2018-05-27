using Photon.Framework.Pooling;

namespace Photon.Library.HttpSecurity
{
    public class HttpUserContext : LifespanReferenceItem
    {
        public string Username {get; set;}
    }
}
