using Photon.Server.Internal.Security;
using Photon.Server.ViewModels.Project;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;

namespace Photon.Server.ViewHandlers.Project
{
    [Secure]
    [RequiresRoles(GroupRole.ProjectEdit)]
    [HttpHandler("/project/json")]
    internal class ProjectJsonHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var projectId = GetQuery("id");

            var vm = new ProjectJsonVM(this) {
                PageTitle = "Photon Server Edit Project JSON",
                ProjectId = projectId,
            };

            vm.Build();

            return Response.View("Project\\Json.html", vm);
        }
    }
}
