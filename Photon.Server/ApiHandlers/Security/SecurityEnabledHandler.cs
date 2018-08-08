using log4net;
using Photon.Library.HttpSecurity;
using Photon.Server.Internal;
using Photon.Server.Internal.Security;
using Photon.Server.Internal.ServerConfiguration;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;
using System;

namespace Photon.Server.ApiHandlers.Security
{
    [Secure]
    [RequiresRoles(GroupRole.SecurityEdit)]
    [HttpHandler("api/security/enabled")]
    internal class SecurityEnabledHandler : HttpHandler
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SecurityEnabledHandler));


        public override HttpHandlerResult Get()
        {
            var config = PhotonServer.Instance.ServerConfiguration;
            var value = config.Value.Security.Enabled;

            return Response.Ok().SetText(value.ToString());
        }

        public override HttpHandlerResult Post()
        {
            var value = GetQuery("value", true);

            try {
                var httpSecurity = (HttpSecurityManager)PhotonServer.Instance.HttpContext.SecurityMgr;
                httpSecurity.Restricted = value;

                var config = PhotonServer.Instance.ServerConfiguration;

                if (config.Value.Security == null)
                    config.Value.Security = new ServerSecurityConfiguration();

                config.Value.Security.Enabled = value;
                config.Save();

                return Response.Ok();
            }
            catch (Exception error) {
                Log.Error("Failed to modify security-enabled value!", error);
                return Response.Exception(error);
            }
        }
    }
}
