using Photon.Library;
using PiServerLite.Http.Handlers;

namespace Photon.Server.HttpHandlers
{
    [HttpHandler("/builds")]
    internal class BuildsHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var vm = new ViewModelBase {
                PageTitle = "Photon Server Builds",
            };

            //try {
            //    vm.Build();
            //}
            //catch (Exception error) {
            //    vm.Errors.Add(error);
            //}

            return Response.View("Builds.html", vm);
        }
    }
}
