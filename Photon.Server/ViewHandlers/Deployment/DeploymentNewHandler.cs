using Photon.Server.ViewModels.Deployment;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;

namespace Photon.Server.ViewHandlers.Deployment
{
    [Secure]
    [HttpHandler("/deployment/new")]
    internal class DeploymentNewHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var vm = new DeploymentNewVM {
                PageTitle = "Photon Server New Deployment",
            };

            //try {
            //    vm.Build();
            //}
            //catch (Exception error) {
            //    vm.Errors.Add(error);
            //}

            return Response.View("Deployment\\New.html", vm);
        }
    }
}
