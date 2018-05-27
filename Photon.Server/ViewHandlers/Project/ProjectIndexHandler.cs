using System;
using Photon.Server.ViewModels.Project;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;

namespace Photon.Server.ViewHandlers.Project
{
    [Secure]
    [HttpHandler("/projects")]
    [HttpHandler("/project/index")]
    internal class ProjectIndexHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var vm = new ProjectIndexVM();

            try {
                vm.Build();
            }
            catch (Exception error) {
                vm.Errors.Add(error);
            }

            return Response.View("Project\\Index.html", vm);
        }
    }
}
