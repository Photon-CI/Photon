using Photon.Library;
using PiServerLite.Http.Handlers;
using System;

namespace Photon.Server.HttpHandlers
{
    [HttpHandler("/deployments")]
    internal class DeploymentsHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var vm = new ViewModelBase();

            try {
                vm.Build();
            }
            catch (Exception error) {
                vm.Errors.Add(error);
            }

            return Response.View("Deployments.html", vm);
        }
    }
}
