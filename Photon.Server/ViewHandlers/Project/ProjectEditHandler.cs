using Photon.Server.ViewModels.Project;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;
using System;

namespace Photon.Server.ViewHandlers.Project
{
    [Secure]
    [HttpHandler("/project/edit")]
    internal class ProjectEditHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var vm = new ProjectEditVM {
                ProjectId = GetQuery("id"),
            };

            var isNew = string.IsNullOrEmpty(vm.ProjectId);
            vm.PageTitle = $"Photon Server {(isNew?"New":"Edit")} Project";

            vm.Build();

            return Response.View("Project\\Edit.html", vm);
        }

        public override HttpHandlerResult Post()
        {
            var vm = new ProjectEditVM();

            try {
                vm.Restore(Request.FormData());
                vm.Save();

                return Response.Redirect("projects");
            }
            catch (Exception error) {
                vm.Errors.Add(error);
                return Response.View("Project\\Edit.html", vm);
            }
        }
    }
}
