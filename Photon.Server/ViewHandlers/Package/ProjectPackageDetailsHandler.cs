using Photon.Server.Internal.Security;
using Photon.Server.ViewModels.Package;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;

namespace Photon.Server.ViewHandlers.Package
{
    [Secure]
    [RequiresRoles(GroupRole.PackagesView)]
    [HttpHandler("/project-package/details")]
    internal class ProjectPackageDetailsHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var vm = new ProjectPackageDetailsVM(this) {
                PackageId = GetQuery("id"),
            };

            vm.Build();

            return Response.View("Package\\ProjectDetails.html", vm);
        }
    }
}
