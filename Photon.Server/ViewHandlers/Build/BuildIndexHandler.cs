using Photon.Server.Internal.Security;
using Photon.Server.ViewModels.Build;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;

namespace Photon.Server.ViewHandlers.Build
{
    [Secure]
    [HttpHandler("/builds")]
    [HttpHandler("/build/index")]
    [RequiresRoles(GroupRole.BuildView)]
    internal class BuildIndexHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var vm = new BuildIndexVM(this) {
                PageTitle = "Photon Server Builds",
            };

            vm.Build();

            return Response.View("Build\\Index.html", vm);
        }
    }
}
