using PiServerLite.Http.Handlers;
using System.Net;

namespace Photon.Library.Http
{
    public class HttpApiHandler : HttpHandler
    {
        public override HttpHandlerResult OnUnauthorized()
        {
            return Response.Status(HttpStatusCode.Unauthorized)
                .SetText("Request Unauthorized!");
        }
    }
}
