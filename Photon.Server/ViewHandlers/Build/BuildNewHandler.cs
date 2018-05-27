using System;
using Photon.Server.ViewModels.Build;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;

namespace Photon.Server.ViewHandlers.Build
{
    [Secure]
    [HttpHandler("/build/new")]
    internal class BuildNewHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var vm = new BuildNewVM {
                PageTitle = "Photon Server New Build",
            };

            try {
                vm.Build();
            }
            catch (Exception error) {
                vm.Errors.Add(error);
            }

            return Response.View("Build\\New.html", vm);
        }
    }
}
