using Photon.Agent.ViewModels;
using PiServerLite.Http.Handlers;

namespace Photon.Agent.ViewHandlers
{
    [HttpHandler("/")]
    [HttpHandler("/index")]
    internal class IndexHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var vm = new IndexVM();

            vm.Build();

            return Response.View("Index.html", vm);
        }
    }
}
