using Photon.Server.ViewModels.Build;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;
using System;

namespace Photon.Server.ViewHandlers.Build
{
    [Secure]
    [HttpHandler("/build/details")]
    internal class BuildDetailsHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var vm = new BuildDetailsVM {
                PageTitle = "Photon Server Build Details",
                ProjectId = GetQuery<string>("project"),
                BuildNumber = GetQuery<uint>("number"),
            };

            try {
                vm.Build();
            }
            catch (Exception error) {
                vm.Errors.Add(error);
            }

            return Response.View("Build\\Details.html", vm);
        }
    }
}
