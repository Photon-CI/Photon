using Photon.Server.Internal.Security;
using Photon.Server.ViewModels.Project;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;

namespace Photon.Server.ViewHandlers.Project
{
    [Secure]
    [RequiresRoles(GroupRole.ProjectView)]
    [HttpHandler("/projects")]
    [HttpHandler("/project/index")]
    internal class ProjectIndexHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var vm = new ProjectIndexVM(this);

            vm.Build();

            return Response.View("Project\\Index.html", vm);
        }
    }
}
