using System;
using Photon.Server.ViewModels.Build;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;

namespace Photon.Server.ViewHandlers.Build
{
    [Secure]
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
