﻿using System;
using Photon.Server.Internal.Security;
using Photon.Server.ViewModels.Security;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;

namespace Photon.Server.ViewHandlers.Security
{
    [Secure]
    [RequiresRoles(GroupRole.SecurityView)]
    [HttpHandler("/security/user/edit")]
    internal class SecurityEditUserHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var qUserId = GetQuery("id");

            var vm = new SecurityEditUserVM(this) {
                UserId = qUserId,
            };

            vm.Build();

            return Response.View("Security\\EditUser.html", vm);
        }

        public override HttpHandlerResult Post()
        {
            var vm = new SecurityEditUserVM(this);

            try {
                vm.Restore(Request.FormData());
                vm.Save();

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
