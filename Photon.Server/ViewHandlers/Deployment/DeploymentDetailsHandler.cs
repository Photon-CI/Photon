using Photon.Server.ViewModels.Deployment;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;
using System;

namespace Photon.Server.ViewHandlers.Deployment
{
    [Secure]
    [HttpHandler("/deployment/details")]
    internal class DeploymentDetailsHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var vm = new DeploymentDetailsVM {
                PageTitle = "Photon Server Deployment Details",
                ProjectId = GetQuery<string>("project"),
                DeploymentNumber = GetQuery<uint>("number"),
            };

            try {
                vm.Build();
            }
            catch (Exception error) {
                vm.Errors.Add(error);
            }

            return Response.View("Deployment\\Details.html", vm);
        }
    }
}
