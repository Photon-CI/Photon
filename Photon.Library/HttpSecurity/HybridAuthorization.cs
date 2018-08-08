using Photon.Library.Security;

namespace Photon.Library.HttpSecurity
{
    public class HybridAuthorization : IAuthorize
    {
        public UserGroupManager UserMgr {get; set;}


        public HttpUserContext Authorize(HttpUserCredentials credentials)
        {
            if (!UserMgr.TryGetUser(credentials.Username, out var user)) return null;

            if (!user.IsEnabled) return null;

            if (user.IsDomainEnabled) {
                // TODO
            }
            else {
                if (!string.Equals(credentials.Password, user.Password)) return null;
            }

            return new HttpUserContext {
                UserId = user.Id,
                Username = user.Username,
            };
        }
    }
}
