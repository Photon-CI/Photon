using Photon.Agent.Internal.Security;
using Photon.Agent.ViewModels.Applications;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;

namespace Photon.Agent.ViewHandlers.Applications
{
    [Secure]
    [RequiresRoles(GroupRole.ApplicationView)]
    [HttpHandler("/application/revisions")]
    [HttpHandler("/application/revision/index")]
    internal class ApplicationRevisionsHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var vm = new ApplicationRevisionsVM(this) {
                ProjectId = GetQuery("project"),
                Name = GetQuery("name"),
            };

            vm.Build();

            return Response.View("Applications\\Revisions.html", vm);
        }
    }
}
