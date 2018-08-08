using Photon.Server.Internal.Security;
using Photon.Server.ViewModels.Security;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;

namespace Photon.Server.ViewHandlers.Security
{
    [Secure]
    [RequiresRoles(GroupRole.SecurityView)]
    [HttpHandler("/security")]
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
