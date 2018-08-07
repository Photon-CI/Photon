using Photon.Server.Internal.Security;
using Photon.Server.ViewModels.Package;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;

namespace Photon.Server.ViewHandlers.Package
{
    [Secure]
    [RequiresRoles(GroupRole.PackagesView)]
    [HttpHandler("/application-package/details")]
    internal class ApplicationPackageDetailsHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var vm = new ApplicationPackageDetailsVM(this) {
                PackageId = GetQuery("id"),
            };

            vm.Build();

            return Response.View("Package\\ApplicationDetails.html", vm);
        }
    }
}
