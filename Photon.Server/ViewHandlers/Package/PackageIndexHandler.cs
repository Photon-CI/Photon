using Photon.Library;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;

namespace Photon.Server.ViewHandlers.Package
{
    [Secure]
    [HttpHandler("/packages")]
    [HttpHandler("/package/index")]
    internal class PackageIndexHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var vm = new ViewModelBase {
                PageTitle = "Photon Server Packages",
            };

            //try {
            //    vm.Build();
            //}
            //catch (Exception error) {
            //    vm.Errors.Add(error);
            //}

            return Response.View("Package\\Index.html", vm);
        }
    }
}
