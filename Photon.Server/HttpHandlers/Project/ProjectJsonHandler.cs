using Photon.Library;
using PiServerLite.Http.Handlers;

namespace Photon.Server.HttpHandlers.Project
{
    [HttpHandler("/projects/json")]
    internal class ProjectJsonHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var vm = new ViewModelBase {
                PageTitle = "Photon Server Projects.json",
            };

            return Response.View("Project\\Json.html", vm);
        }
    }
}
