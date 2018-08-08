using Photon.Server.Internal;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;

namespace Photon.Server.ViewHandlers
{
    [Secure]
    [HttpHandler("/accessDenied")]
    internal class AccessDeniedHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var vm = new ServerViewModel(this) {
                PageTitle = "Photon Server Access Denied",
            };

            vm.Build();

            return Response.View("AccessDenied.html", vm);
        }
    }
}
