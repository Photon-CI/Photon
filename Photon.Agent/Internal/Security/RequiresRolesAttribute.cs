using PiServerLite.Http.Handlers;
using System.Linq;
using Photon.Library.Http.Security;

namespace Photon.Agent.Internal.Security
{
    internal class RequiresRolesAttribute : HttpFilterAttribute
    {
        public string[] Roles;


        public RequiresRolesAttribute(params string[] roles)
        {
            this.Roles = roles;
            Events = HttpFilterEvents.Before;
            Function = OnPreFilter;
        }

        public HttpHandlerResult OnPreFilter(IHttpHandler httpHandler)
        {
            var isSecurityEnabled = PhotonAgent.Instance.AgentConfiguration.Value.Security?.Enabled ?? false;
            if (!isSecurityEnabled) return null;

            var httpSecurity = (HttpSecurityManager)PhotonAgent.Instance.HttpContext.SecurityMgr;
            if (!httpSecurity.TryGetUserContext(httpHandler.HttpContext.Request, out var userContext))
                return httpHandler.Response.Redirect("AccessDenied");

            var rolesRequired = httpHandler.GetType()
                .GetCustomAttributes(true)
                .OfType<RequiresRolesAttribute>()
                .SelectMany(x => x.Roles)
                .Distinct().ToArray();

            var hasAccess = !rolesRequired.Any() || PhotonAgent.Instance.UserMgr.UserHasRoles(userContext.UserId, rolesRequired);

            return hasAccess ? null : httpHandler.Response.Redirect("AccessDenied");
        }
    }
}
