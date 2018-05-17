using Photon.Library;
using PiServerLite.Http.Handlers;

namespace Photon.Server.HttpHandlers.Projects
{
    [HttpHandler("/projects/json")]
    internal class ProjectJsonHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var vm = new ViewModelBase {
                PageTitle = "Photon Server Projects.json",
            };

            return Response.View("Projects\\Json.html", vm);
        }
    }
}
