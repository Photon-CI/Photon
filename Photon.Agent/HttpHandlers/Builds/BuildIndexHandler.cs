using Photon.Library;
using PiServerLite.Http.Handlers;
using System;

namespace Photon.Agent.HttpHandlers.Builds
{
    [HttpHandler("/builds")]
    [HttpHandler("/build/index")]
    internal class BuildIndexHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var vm = new ViewModelBase();

            try {
                vm.Build();
            }
            catch (Exception error) {
                vm.Errors.Add(error);
            }

            return Response.View("Builds\\Index.html", vm);
        }
    }
}
