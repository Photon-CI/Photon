using Photon.Agent.Internal.Security;
using Photon.Agent.ViewModels.Configuration;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;

namespace Photon.Agent.ViewHandlers.Configuration
{
    [Secure]
    [RequiresRoles(GroupRole.ConfigurationView)]
    [HttpHandler("/configuration")]
    [HttpHandler("/configuration/index")]
    internal class ConfigurationIndexHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var vm = new ConfigurationIndexVM(this);

            vm.Build();

            return Response.View("Configuration\\Index.html", vm);
        }
    }
}
