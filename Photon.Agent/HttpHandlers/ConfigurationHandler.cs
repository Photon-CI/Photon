using Photon.Library;
using PiServerLite.Http.Handlers;
using System;

namespace Photon.Agent.HttpHandlers
{
    [HttpHandler("/configuration")]
    internal class ConfigurationHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var vm = new ViewModelBase {
                PageTitle = "Photon Agent Configuration"
            };

            try {
                vm.Build();
            }
            catch (Exception error) {
                vm.Errors.Add(error);
            }

            return Response.View("Configuration.html", vm);
        }
    }
}
