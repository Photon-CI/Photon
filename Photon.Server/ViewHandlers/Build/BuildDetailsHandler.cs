using Photon.Server.Internal.Security;
using Photon.Server.ViewModels.Build;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;

namespace Photon.Server.ViewHandlers.Build
{
    [Secure]
    [HttpHandler("/build/details")]
    [RequiresRoles(GroupRole.BuildView)]
    internal class BuildDetailsHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var vm = new BuildDetailsVM(this) {
                PageTitle = "Photon Server Build Details",
                ProjectId = GetQuery("project"),
                BuildNumber = GetQuery<uint>("number"),
            };

            vm.Build();

            return Response.View("Build\\Details.html", vm);
        }
    }
}
