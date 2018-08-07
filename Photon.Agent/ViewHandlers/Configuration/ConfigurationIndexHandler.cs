using Photon.Agent.ViewModels.Configuration;
using PiServerLite.Http.Handlers;

namespace Photon.Agent.ViewHandlers.Configuration
{
    [HttpHandler("/configuration")]
    [HttpHandler("/configuration/index")]
    internal class ConfigurationIndexHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var vm = new ConfigurationIndexVM();

            return Response.View("Configuration\\Index.html", vm);
        }
    }
}
