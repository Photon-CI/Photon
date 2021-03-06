﻿using Photon.Server.Internal.Security;
using Photon.Server.ViewModels.Build;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Server.ViewHandlers.Build
{
    [Secure]
    [HttpHandler("/build/new")]
    [RequiresRoles(GroupRole.BuildStart)]
    internal class BuildNewHandler : HttpHandlerAsync
    {
        public override Task<HttpHandlerResult> GetAsync(CancellationToken token)
        {
            var vm = new BuildNewVM(this) {
                ProjectId = GetQuery("project"),
                TaskName = GetQuery("task"),
                TaskRoles = GetQuery("roles"),
                PreBuildCommand = GetQuery("prebuild"),
                AssemblyFilename = GetQuery("assembly"),
            };

            var qRefspec = GetQuery("refspec");
            if (!string.IsNullOrEmpty(qRefspec))
                vm.GitRefspec = qRefspec;

            vm.Build();

            return Response.View("Build\\New.html", vm).AsAsync();
        }

        public override async Task<HttpHandlerResult> PostAsync(CancellationToken token)
        {
            var vm = new BuildNewVM(this);

            try {
                vm.Restore(Request.FormData());
                await vm.StartBuild();

                // TODO: Add support for url fragments to PiServerLite
                // TODO: Then nav to #output
                return Response.Redirect("build/details", new {
                    project = vm.ProjectId,
                    number = vm.BuildNumber,
                });
            }
            catch (Exception error) {
                vm.Errors.Add(error);

                vm.Build();

                return Response.View("Build\\New.html", vm);
            }
        }
    }
}
