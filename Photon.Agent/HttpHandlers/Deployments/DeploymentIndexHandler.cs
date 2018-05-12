using Photon.Library;
using PiServerLite.Http.Handlers;

namespace Photon.Agent.HttpHandlers.Deployments
{
    [HttpHandler("/deployments")]
    [HttpHandler("/deployment/index")]
    internal class DeploymentIndexHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var vm = new ViewModelBase {
                PageTitle = "Photon Agent Deployments",
            };

            vm.Build();

            return Response.View("Deployments\\Index.html", vm);
        }
    }
}
