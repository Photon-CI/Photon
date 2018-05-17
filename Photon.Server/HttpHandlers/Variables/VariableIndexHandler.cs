using Photon.Library;
using PiServerLite.Http.Handlers;

namespace Photon.Server.HttpHandlers.Variables
{
    [HttpHandler("/variables")]
    [HttpHandler("/variable/index")]
    internal class VariableIndexHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var vm = new ViewModelBase {
                PageTitle = "Photon Server Variables",
            };

            //try {
            //    vm.Build();
            //}
            //catch (Exception error) {
            //    vm.Errors.Add(error);
            //}

            return Response.View("Variables\\Index.html", vm);
        }
    }
}
