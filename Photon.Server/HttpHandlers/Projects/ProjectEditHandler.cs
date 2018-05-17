using Photon.Server.ViewModels.Projects;
using PiServerLite.Http.Handlers;
using System;

namespace Photon.Server.HttpHandlers.Projects
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

            return Response.View("Projects\\Edit.html", vm);
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
                return Response.View("Projects\\Edit.html", vm);
            }
        }
    }
}
