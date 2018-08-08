using Photon.Agent.Internal.Security;
using Photon.Agent.ViewModels.Applications;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;

namespace Photon.Agent.ViewHandlers.Applications
{
    [Secure]
    [RequiresRoles(GroupRole.ApplicationView)]
    [HttpHandler("/applications")]
    [HttpHandler("/application/index")]
    internal class ApplicationIndexHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var vm = new ApplicationIndexVM(this);

            vm.Build();

            return Response.View("Applications\\Index.html", vm);
        }
    }
}
