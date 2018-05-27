using Photon.Library;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;

namespace Photon.Server.ViewHandlers
{
    [Secure]
    [HttpHandler("/configuration")]
    internal class ConfigurationHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var vm = new ViewModelBase {
                PageTitle = "Photon Server Configuration",
            };

            //try {
            //    vm.Build();
            //}
            //catch (Exception error) {
            //    vm.Errors.Add(error);
            //}

            return Response.View("Configuration.html", vm);
        }
    }
}
