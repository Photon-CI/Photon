using System;
using Photon.Server.ViewModels;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;

namespace Photon.Server.ViewHandlers
{
    [Secure]
    [HttpHandler("/")]
    [HttpHandler("/index")]
    internal class IndexHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var vm = new IndexVM();

            try {
                vm.Build();
            }
            catch (Exception error) {
                vm.Errors.Add(error);
            }

            return Response.View("Index.html", vm);
        }
    }
}
