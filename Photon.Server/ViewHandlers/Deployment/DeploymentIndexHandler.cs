using Photon.Server.Internal.Security;
using Photon.Server.ViewModels.Deployment;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;

namespace Photon.Server.ViewHandlers.Deployment
{
    [Secure]
    [RequiresRoles(GroupRole.DeployView)]
    [HttpHandler("/deployments")]
    [HttpHandler("/deployment/index")]
    internal class DeploymentIndexHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var vm = new DeploymentIndexVM(this) {
                PageTitle = "Photon Server Deployments",
            };

            vm.Build();

            return Response.View("Deployment\\Index.html", vm);
        }
    }
}
