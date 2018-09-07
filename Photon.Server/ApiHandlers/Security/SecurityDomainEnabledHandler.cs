using log4net;
using Photon.Library.Http;
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
    [HttpHandler("api/security/domain/enabled")]
    internal class SecurityDomainEnabledHandler : HttpApiHandler
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SecurityDomainEnabledHandler));


        public override HttpHandlerResult Get()
        {
            var config = PhotonServer.Instance.ServerConfiguration;
            var value = config.Value.Security.DomainEnabled;

            return Response.Ok().SetText(value.ToString());
        }

        public override HttpHandlerResult Post()
        {
            var value = GetQuery("value", true);

            try {
                PhotonServer.Instance.Http.Authorization.DomainEnabled = value;

                var config = PhotonServer.Instance.ServerConfiguration;

                if (config.Value.Security == null)
                    config.Value.Security = new ServerSecurityConfiguration();

                config.Value.Security.DomainEnabled = value;
                config.Save();

                return Response.Ok();
            }
            catch (Exception error) {
                Log.Error("Failed to modify security domain-enabled value!", error);
                return Response.Exception(error);
            }
        }
    }
}
