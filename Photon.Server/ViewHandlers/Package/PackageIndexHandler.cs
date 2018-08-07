using Photon.Server.Internal.Security;
using Photon.Server.ViewModels.Package;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;

namespace Photon.Server.ViewHandlers.Package
{
    [Secure]
    [RequiresRoles(GroupRole.PackagesView)]
    [HttpHandler("/packages")]
    [HttpHandler("/package/index")]
    internal class PackageIndexHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var vm = new PackageIndexVM(this);

            vm.Build();

            return Response.View("Package\\Index.html", vm);
        }
    }
}
