using Photon.Server.ViewModels.Projects;
using PiServerLite.Http.Handlers;

namespace Photon.Server.HttpHandlers.Projects
{
    [HttpHandler("/projects")]
    [HttpHandler("/project/index")]
    internal class ProjectIndexHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var vm = new ProjectIndexVM();

            vm.Build();

            return View("Projects\\Index.html", vm);
        }
    }
}
