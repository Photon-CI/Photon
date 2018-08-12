using Photon.Server.Internal.Security;
using Photon.Server.ViewModels.Security;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;
using System;

namespace Photon.Server.ViewHandlers.Security
{
    [Secure]
    [RequiresRoles(GroupRole.SecurityEdit)]
    [HttpHandler("/security/user/delete")]
    internal class SecurityDeleteUserHandler : HttpHandler
    {
        public override HttpHandlerResult Post()
        {
            var vm = new SecurityEditUserVM(this);

            try {
                vm.Restore(Request.FormData());
                vm.Delete();

                return Response.Redirect("security");
            }
            catch (Exception error) {
                vm.Errors.Add(error);

                vm.Build();

                return Response.View("Security\\EditUser.html", vm);
            }
        }
    }
}
