using Photon.Server.ViewModels.Package;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;
using System;

namespace Photon.Server.ViewHandlers.Package
{
    [Secure]
    [HttpHandler("/project-package/details")]
    internal class ProjectPackageDetailsHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var vm = new ProjectPackageDetailsVM {
                PackageId = GetQuery("id"),
            };

            try {
                vm.Build();
            }
            catch (Exception error) {
                vm.Errors.Add(error);
            }

            return Response.View("Package\\ProjectDetails.html", vm);
        }
    }
}
