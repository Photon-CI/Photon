﻿using Photon.Server.ViewModels.Build;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Server.ViewHandlers.Build
{
    [Secure]
    [HttpHandler("/build/new")]
    internal class BuildNewHandler : HttpHandlerAsync
    {
        public override Task<HttpHandlerResult> GetAsync(CancellationToken token)
        {
            var vm = new BuildNewVM {
                ProjectId = GetQuery("project"),
            };

            try {
                vm.Build();
            }
            catch (Exception error) {
                vm.Errors.Add(error);
            }

            return Response.View("Build\\New.html", vm).AsAsync();
        }

        public override async Task<HttpHandlerResult> PostAsync(CancellationToken token)
        {
            var vm = new BuildNewVM();

            try {
                vm.Restore(Request.FormData());
                await vm.StartBuild();

                return Response.Redirect("build/details", new {
                    project = vm.ProjectId,
                    number = vm.BuildNumber,
                });
            }
            catch (Exception error) {
                vm.Errors.Add(error);
                return Response.View("Build\\New.html", vm);
            }
        }
    }
}
