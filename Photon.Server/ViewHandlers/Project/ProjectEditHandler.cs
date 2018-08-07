using Photon.Server.ViewModels.Project;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;
using System;
using Photon.Server.Internal.Security;

namespace Photon.Server.ViewHandlers.Project
{
    [Secure]
    [RequiresRoles(GroupRole.ProjectEdit)]
    [HttpHandler("/project/edit")]
    internal class ProjectEditHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var vm = new ProjectEditVM(this) {
                ProjectId = GetQuery("id"),
            };

            var isNew = string.IsNullOrEmpty(vm.ProjectId);
            vm.PageTitle = $"Photon Server {(isNew?"New":"Edit")} Project";

            vm.Build();

            return Response.View("Project\\Edit.html", vm);
        }

        public override HttpHandlerResult Post()
        {
            var vm = new ProjectEditVM(this);

            try {
                vm.Restore(Request.FormData());
                vm.Save();

                return Response.Redirect("projects");
            }
            catch (Exception error) {
                vm.Errors.Add(error);

                vm.Build();

                return Response.View("Project\\Edit.html", vm);
            }
        }
    }
}
