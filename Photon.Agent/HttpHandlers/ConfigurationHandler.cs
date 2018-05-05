using Photon.Library;
using PiServerLite.Http.Handlers;

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
            vm.Build();

            return View("Configuration.html", vm);
        }
    }
}
