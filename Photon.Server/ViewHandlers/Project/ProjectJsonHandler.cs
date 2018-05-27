using Photon.Server.ViewModels.Project;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;

namespace Photon.Server.ViewHandlers.Project
{
    [Secure]
    [HttpHandler("/project/json")]
    internal class ProjectJsonHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var projectId = GetQuery("id");

            var vm = new ProjectJsonVM {
                PageTitle = "Photon Server Edit Project JSON",
                ProjectId = projectId,
            };

            return Response.View("Project\\Json.html", vm);
        }
    }
}
