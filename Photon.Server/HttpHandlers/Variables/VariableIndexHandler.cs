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

            vm.Build();

            return View("Variables\\Index.html", vm);
        }
    }
}
