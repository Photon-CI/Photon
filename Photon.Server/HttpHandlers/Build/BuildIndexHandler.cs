using Photon.Server.ViewModels.Build;
using PiServerLite.Http.Handlers;
using System;

namespace Photon.Server.HttpHandlers.Build
{
    [HttpHandler("/builds")]
    [HttpHandler("/build/index")]
    internal class BuildIndexHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var vm = new BuildIndexVM {
                PageTitle = "Photon Server Builds",
            };

            try {
                vm.Build();
            }
            catch (Exception error) {
                vm.Errors.Add(error);
            }

            return Response.View("Build\\Index.html", vm);
        }
    }
}
