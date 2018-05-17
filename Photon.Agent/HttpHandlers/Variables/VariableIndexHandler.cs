using Photon.Library;
using PiServerLite.Http.Handlers;

namespace Photon.Agent.HttpHandlers.Variables
{
    [HttpHandler("/variables")]
    [HttpHandler("/variable/index")]
    internal class VariableIndexHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var vm = new ViewModelBase {
                PageTitle = "Photon Agent Variables",
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
