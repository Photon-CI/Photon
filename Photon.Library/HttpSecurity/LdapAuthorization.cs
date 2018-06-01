using System;

namespace Photon.Library.HttpSecurity
{
    public class LdapAuthorization : IAuthorize
    {
        public HttpUserContext Authorize(HttpUserCredentials credentials)
        {
            if (!string.Equals("photon", credentials.Username, StringComparison.OrdinalIgnoreCase)) return null;
            if (!string.Equals("ci", credentials.Password, StringComparison.Ordinal)) return null;

            return new HttpUserContext {
                Username = "photon",
            };
        }
    }
}
