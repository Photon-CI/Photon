using PiServerLite.Http.Handlers;
using System;
using Photon.Server.ViewModels.Project;

namespace Photon.Server.HttpHandlers.Project
{
    [HttpHandler("/project/edit")]
    internal class ProjectEditHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var vm = new ProjectEditVM {
                ProjectId = GetQuery("id"),
            };

            vm.PageTitle = $"Photon Server {(vm.IsNew?"New":"Edit")} Project";

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
