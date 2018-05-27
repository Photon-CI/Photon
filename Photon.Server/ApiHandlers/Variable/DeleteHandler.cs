using Photon.Server.Internal;
using PiServerLite.Http.Handlers;

namespace Photon.Server.ApiHandlers.Variable
{
    [HttpHandler("/api/variable/delete")]
    internal class DeleteHandler : HttpHandler
    {
        public override HttpHandlerResult Post()
        {
            var id = GetQuery("id");

            if (string.IsNullOrEmpty(id))
                return Response.BadRequest().SetText("'id' is undefined!");

            if (!PhotonServer.Instance.Variables.Remove(id))
                return Response.BadRequest().SetText("Failed to remove Variable Set!");

            return Response.Ok();
        }
    }
}
