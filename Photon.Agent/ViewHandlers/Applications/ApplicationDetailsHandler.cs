using Photon.Agent.Internal.Security;
using Photon.Agent.ViewModels.Applications;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;

namespace Photon.Agent.ViewHandlers.Applications
{
    [Secure]
    [RequiresRoles(GroupRole.ApplicationView)]
    [HttpHandler("/application/details")]
    internal class ApplicationDetailsHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var vm = new ApplicationDetailsVM(this) {
                ProjectId = GetQuery("project"),
                Name = GetQuery("name"),
            };

            vm.Build();

            return Response.View("Applications\\Details.html", vm);
        }
    }
}
