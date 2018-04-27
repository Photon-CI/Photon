using PiServerLite.Http.Handlers;
using System.Threading.Tasks;

namespace Photon.Server.HttpHandlers.Api.GitHub
{
    [HttpHandler("api/github/webhook")]
    internal class WebHookHandler : HttpHandlerAsync
    {
        public override async Task<HttpHandlerResult> PostAsync()
        {
            var eventType = HttpContext.Request.Headers["X-GitHub-Event"];
            var deliveryId = HttpContext.Request.Headers["X-GitHub-Delivery"];
            var signature = HttpContext.Request.Headers["X-Hub-Signature"];

            //...

            return Ok();
        }
    }
}
