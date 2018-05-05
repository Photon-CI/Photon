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

            vm.Build();

            return View("Variables\\Index.html", vm);
        }
    }
}
