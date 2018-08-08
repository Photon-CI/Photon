using Photon.Agent.Internal.Security;
using Photon.Agent.ViewModels.Security;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;

namespace Photon.Agent.ViewHandlers.Security
{
    [Secure]
    [RequiresRoles(GroupRole.SecurityView)]
    [HttpHandler("/security")]
    [HttpHandler("/security/index")]
    internal class SecurityIndexHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var vm = new SecurityIndexVM(this);

            vm.Build();

            return Response.View("Security\\Index.html", vm);
        }
    }
}
