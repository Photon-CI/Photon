using Photon.Agent.Internal;
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

            var vm = new LoginVM {
                Username = _user,
                RememberMe = _user != null,
            };

            return Response.View("Login.html", vm);
        }

        public override HttpHandlerResult Post()
        {
            var vm = new LoginVM();
            vm.Restore(Request.FormData());

            var user = new HttpUserCredentials {
                Username = vm.Username,
                Password = vm.Password,
            };

            var serverSecurity = (AgentHttpSecurity) Context.SecurityMgr;
            if (!serverSecurity.Authorize(HttpContext.Response, user)) {
                vm.AuthMessage = "Invalid Credentials";
                return Response.View("Login.html", vm);
            }

            var returnUrl = GetQuery("returnUrl");

            if (!string.IsNullOrEmpty(returnUrl))
                return Response.RedirectUrl(returnUrl);

            return Response.Redirect("/index");
        }
    }
}
