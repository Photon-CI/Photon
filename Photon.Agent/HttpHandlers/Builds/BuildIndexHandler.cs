using Photon.Library;
using PiServerLite.Http.Handlers;

namespace Photon.Agent.HttpHandlers.Builds
{
    [HttpHandler("/builds")]
    [HttpHandler("/build/index")]
    internal class BuildIndexHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var vm = new ViewModelBase {
                PageTitle = "Photon Agent Builds"
            };

            //try {
            //    vm.Build();
            //}
            //catch (Exception error) {
            //    vm.Errors.Add(error);
            //}

            return Response.View("Builds\\Index.html", vm);
        }
    }
}
