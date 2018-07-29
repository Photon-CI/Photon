using Photon.Library;
using PiServerLite.Http.Handlers;

namespace Photon.Agent.ViewHandlers.Configuration
{
    [HttpHandler("/configuration")]
    internal class ConfigurationIndexHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var vm = new ViewModelBase {
                PageTitle = "Photon Agent Configuration"
            };

            //try {
            //    vm.Build();
            //}
            //catch (Exception error) {
            //    vm.Errors.Add(error);
            //}

            return Response.View("Configuration\\Index.html", vm);
        }
    }
}
