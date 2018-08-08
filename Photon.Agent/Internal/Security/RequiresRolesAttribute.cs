using PiServerLite.Http.Handlers;
using System.Linq;

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

            var httpSecurity = (AgentHttpSecurity)PhotonAgent.Instance.HttpContext.SecurityMgr;
            if (!httpSecurity.GetUserContext(httpHandler.HttpContext.Request, out var userContext))
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
