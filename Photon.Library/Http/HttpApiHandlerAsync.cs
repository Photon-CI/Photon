using PiServerLite.Http.Handlers;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Library.Http
{
    public class HttpApiHandlerAsync : HttpHandlerAsync
    {
        public override async Task<HttpHandlerResult> OnUnauthorizedAsync(CancellationToken token)
        {
            return await Response.Status(HttpStatusCode.Unauthorized)
                .SetText("Request Unauthorized!").AsAsync();
        }
    }
}
