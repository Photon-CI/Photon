using Photon.Agent.Internal.Security;
using Photon.Agent.ViewModels;
using Photon.Library.HttpSecurity;
using PiServerLite.Http.Handlers;

namespace Photon.Agent.ViewHandlers
{
    [HttpHandler("/login")]
    internal class LoginHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var _user = HttpContext.Request.Cookies["photon.agent.username"]?.Value;

            var vm = new LoginVM(this) {
                Username = _user,
                RememberMe = _user != null,
            };

            vm.Build();

            return Response.View("Login.html", vm);
        }

        public override HttpHandlerResult Post()
        {
            var vm = new LoginVM(this);
            vm.Restore(Request.FormData());

            var user = new HttpUserCredentials {
                Username = vm.Username,
                Password = vm.Password,
            };

            var serverSecurity = (AgentHttpSecurity) Context.SecurityMgr;
            if (!serverSecurity.Authorize(HttpContext.Response, user)) {
                vm.AuthMessage = "Invalid Credentials";
                vm.Build();

                return Response.View("Login.html", vm);
            }

            var returnUrl = GetQuery("returnUrl");

            if (!string.IsNullOrEmpty(returnUrl))
                return Response.RedirectUrl(returnUrl);

            return Response.Redirect("/index");
        }
    }
}
