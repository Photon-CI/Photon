using Photon.Server.Internal.Security;
using Photon.Server.ViewModels.Deployment;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;

namespace Photon.Server.ViewHandlers.Deployment
{
    [Secure]
    [RequiresRoles(GroupRole.DeployView)]
    [HttpHandler("/deployment/details")]
    internal class DeploymentDetailsHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var vm = new DeploymentDetailsVM(this) {
                PageTitle = "Photon Server Deployment Details",
                ProjectId = GetQuery<string>("project"),
                DeploymentNumber = GetQuery<uint>("number"),
            };

            vm.Build();

            return Response.View("Deployment\\Details.html", vm);
        }
    }
}
