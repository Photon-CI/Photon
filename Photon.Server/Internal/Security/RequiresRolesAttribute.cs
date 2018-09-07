using PiServerLite.Http.Handlers;
using System.Linq;

namespace Photon.Server.Internal.Security
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
            var isSecurityEnabled = PhotonServer.Instance.ServerConfiguration.Value.Security?.Enabled ?? false;
            if (!isSecurityEnabled) return null;

            if (!PhotonServer.Instance.Http.Security.TryGetUserContext(httpHandler.HttpContext.Request, out var userContext))
                return httpHandler.Response.Redirect("AccessDenied");

            var rolesRequired = httpHandler.GetType()
                .GetCustomAttributes(true)
                .OfType<RequiresRolesAttribute>()
                .SelectMany(x => x.Roles)
                .Distinct().ToArray();

            var hasAccess = !rolesRequired.Any() || PhotonServer.Instance.UserMgr.UserHasRoles(userContext.UserId, rolesRequired);

            return hasAccess ? null : httpHandler.Response.Redirect("AccessDenied");
        }
    }
}
