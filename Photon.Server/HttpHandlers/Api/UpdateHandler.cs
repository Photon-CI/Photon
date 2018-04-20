using PiServerLite.Http.Handlers;
using System;
using System.Threading.Tasks;

namespace Photon.Server.HttpHandlers.Api
{
    [HttpHandler("/api/update")]
    internal class UpdateHandler : HttpHandlerAsync
    {
        public override async Task<HttpHandlerResult> PostAsync()
        {
            // TODO: Create a new 'UpdateSession' class?

            throw new NotImplementedException();

            return await Task.FromResult(Ok());
        }
    }
}
