using Photon.Library.HttpSecurity;
using Photon.Server.Internal.Security;
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

            var serverSecurity = (ServerHttpSecurity) Context.SecurityMgr;
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
