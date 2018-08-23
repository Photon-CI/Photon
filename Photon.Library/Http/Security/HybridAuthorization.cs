using System.DirectoryServices.AccountManagement;
using Photon.Library.Security;

namespace Photon.Library.Http.Security
{
    public class HybridAuthorization : IAuthorize
    {
        public UserGroupManager UserMgr {get; set;}
        public bool DomainEnabled {get; set;}


        public HttpUserContext Authorize(HttpUserCredentials credentials)
        {
            // TODO: To support domain groups, this check must be moved!
            if (!UserMgr.TryGetUserByUsername(credentials.Username, out var user)) return null;

            if (!user.IsEnabled) return null;

            if (user.IsDomainEnabled && DomainEnabled) {
                using (var domain = new PrincipalContext(ContextType.Domain))
                using (var domainUser = UserPrincipal.FindByIdentity(domain, credentials.Username)) {
                    if (domainUser == null) return null;

                    if (!domain.ValidateCredentials(credentials.Username, credentials.Password)) return null;

                    // Check if the user maps directly to a domain user.
                    // If not, check if any of the domain user's groups
                    // maps to a photon user.
                }
            }
            else {
                if (string.IsNullOrEmpty(user.Password)) return null;
                if (!string.Equals(credentials.Password, user.Password)) return null;
            }

            return new HttpUserContext {
                UserId = user.Id,
                Username = user.Username,
            };
        }
    }
}
