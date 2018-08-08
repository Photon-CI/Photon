using Photon.Agent.Internal;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;

namespace Photon.Agent.ViewHandlers
{
    [Secure]
    [HttpHandler("/accessDenied")]
    internal class AccessDeniedHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var vm = new AgentViewModel(this) {
                PageTitle = "Photon Agent Access Denied",
            };

            vm.Build();

            return Response.View("AccessDenied.html", vm);
        }
    }
}
