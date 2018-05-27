using Photon.Library;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;

namespace Photon.Server.ViewHandlers.Deployment
{
    [Secure]
    [HttpHandler("/deployments")]
    [HttpHandler("/deployment/index")]
    internal class DeploymentIndexHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var vm = new ViewModelBase {
                PageTitle = "Photon Server Deployments",
            };

            //try {
            //    vm.Build();
            //}
            //catch (Exception error) {
            //    vm.Errors.Add(error);
            //}

            return Response.View("Deployment\\Index.html", vm);
        }
    }
}
