using Photon.Agent.Internal.Security;
using Photon.Agent.ViewModels.Configuration;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;
using System;

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
            vm.Load();

            return Response.View("Configuration\\Index.html", vm);
        }

        public override HttpHandlerResult Post()
        {
            var vm = new ConfigurationIndexVM(this);

            try {
                vm.Restore(Request.FormData());
                vm.Save();

                // TODO: Would be really cool to automatically redirect to the new URI
            }
            catch (Exception error) {
                vm.Errors.Add(error);
            }

            vm.Build();
            return Response.View("Configuration\\Index.html", vm);
        }
    }
}
