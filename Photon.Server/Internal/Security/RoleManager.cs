using System.Linq;

namespace Photon.Server.Internal.Security
{
    internal static class RoleManager
    {
        public static bool HasAccess(object httpHandler)
        {
            // TODO: Get real user-id
            var userId = "_admin_";

            var rolesRequired = httpHandler.GetType().GetCustomAttributes(true)
                .OfType<RequiresRolesAttribute>()
                .SelectMany(x => x.Roles).ToArray();

            return !rolesRequired.Any() || PhotonServer.Instance.UserMgr.UserHasRoles(userId, rolesRequired);
        }
    }
}
