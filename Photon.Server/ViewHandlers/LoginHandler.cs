using Photon.Library.HttpSecurity;
using Photon.Server.Internal;
using Photon.Server.ViewModels;
using PiServerLite.Http.Handlers;

namespace Photon.Server.ViewHandlers
{
    [HttpHandler("/login")]
    internal class LoginHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var _user = HttpContext.Request.Cookies["photon.server.username"]?.Value;

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

            var serverSecurity = (ServerHttpSecurity) Context.SecurityMgr;
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
