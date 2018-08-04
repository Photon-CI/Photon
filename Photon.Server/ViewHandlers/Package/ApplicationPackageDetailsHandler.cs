using Photon.Server.ViewModels.Package;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;
using System;

namespace Photon.Server.ViewHandlers.Package
{
    [Secure]
    [HttpHandler("/application-package/details")]
    internal class ApplicationPackageDetailsHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var vm = new ApplicationPackageDetailsVM {
                PackageId = GetQuery("id"),
            };

            try {
                vm.Build();
            }
            catch (Exception error) {
                vm.Errors.Add(error);
            }

            return Response.View("Package\\ApplicationDetails.html", vm);
        }
    }
}
