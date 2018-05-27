using System;

namespace Photon.Library.HttpSecurity
{
    public class LdapAuthorization : IAuthorize
    {
        public HttpUserContext Authorize(HttpUserCredentials credentials)
        {
            if (!string.Equals("user", credentials.Username, StringComparison.OrdinalIgnoreCase)) return null;
            if (!string.Equals("pass", credentials.Password, StringComparison.Ordinal)) return null;

            return new HttpUserContext {
                Username = "UserPass",
            };
        }
    }
}
