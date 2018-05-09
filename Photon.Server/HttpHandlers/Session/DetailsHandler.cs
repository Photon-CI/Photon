using Photon.Library;
using PiServerLite.Http.Handlers;

namespace Photon.Server.HttpHandlers.Session
{
    [HttpHandler("session/details")]
    internal class DetailsHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var sessionId = GetQuery("id");

            var vm = new ViewModelBase {
                PageTitle = "Photon Server Session Details",
            };

            //...

            return View("Session\\Details.html", vm);
        }
    }
}
