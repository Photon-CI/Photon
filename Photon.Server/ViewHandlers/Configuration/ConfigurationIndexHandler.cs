using Photon.Server.Internal.Security;
using Photon.Server.ViewModels.Configuration;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;

namespace Photon.Server.ViewHandlers.Configuration
{
    [Secure]
    [RequiresRoles(GroupRole.ConfigurationView)]
    [HttpHandler("/configuration")]
    internal class ConfigurationHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var vm = new ConfigurationIndexVM(this);

            vm.Build();

            return Response.View("Configuration\\Index.html", vm);
        }
    }
}
