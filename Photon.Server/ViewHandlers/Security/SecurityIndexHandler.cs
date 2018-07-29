using Photon.Library;
using PiServerLite.Http.Handlers;

namespace Photon.Server.ViewHandlers.Security
{
    [HttpHandler("/security")]
    internal class SecurityIndexHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var vm = new ViewModelBase {
                PageTitle = "Photon Server Security"
            };

            //try {
            //    vm.Build();
            //}
            //catch (Exception error) {
            //    vm.Errors.Add(error);
            //}

            return Response.View("Security\\Index.html", vm);
        }
    }
}
