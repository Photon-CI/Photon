using log4net;
using Photon.Agent.Internal;
using Photon.Agent.Internal.AgentConfiguration;
using Photon.Agent.Internal.Security;
using Photon.Library.Http;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;
using System;

namespace Photon.Agent.ApiHandlers.Security
{
    [Secure]
    [RequiresRoles(GroupRole.SecurityEdit)]
    [HttpHandler("api/security/domain/enabled")]
    internal class SecurityDomainEnabledHandler : HttpApiHandler
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SecurityDomainEnabledHandler));


        public override HttpHandlerResult Get()
        {
            var config = PhotonAgent.Instance.AgentConfiguration;
            var value = config.Value.Security.DomainEnabled;

            return Response.Ok().SetText(value.ToString());
        }

        public override HttpHandlerResult Post()
        {
            var value = GetQuery("value", true);

            try {
                PhotonAgent.Instance.Http.Authorization.DomainEnabled = value;

                var config = PhotonAgent.Instance.AgentConfiguration;

                if (config.Value.Security == null)
                    config.Value.Security = new AgentSecurityConfiguration();

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
