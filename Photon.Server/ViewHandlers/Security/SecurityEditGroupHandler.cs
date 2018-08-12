using System;
using Photon.Server.Internal.Security;
using Photon.Server.ViewModels.Security;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;

namespace Photon.Server.ViewHandlers.Security
{
    [Secure]
    [RequiresRoles(GroupRole.SecurityView)]
    [HttpHandler("/security/group/edit")]
    internal class SecurityEditGroupHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var qGroupId = GetQuery("id");

            var vm = new SecurityEditGroupVM(this) {
                GroupId = qGroupId,
            };

            vm.Build();

            return Response.View("Security\\EditGroup.html", vm);
        }

        public override HttpHandlerResult Post()
        {
            var vm = new SecurityEditGroupVM(this);

            try {
                vm.Restore(Request.FormData());
                vm.Save();

                return Response.Redirect("security");
            }
            catch (Exception error) {
                vm.Errors.Add(error);

                vm.Build();

                return Response.View("Security\\EditGroup.html", vm);
            }
        }
    }
}
