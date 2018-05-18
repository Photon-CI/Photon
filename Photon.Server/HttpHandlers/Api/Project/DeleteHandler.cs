using Photon.Server.Internal;
using PiServerLite.Http.Handlers;

namespace Photon.Server.HttpHandlers.Api.Project
{
    [HttpHandler("/api/project/delete")]
    internal class DeleteHandler : HttpHandler
    {
        public override HttpHandlerResult Post()
        {
            var id = GetQuery("id");

            if (string.IsNullOrEmpty(id))
                return Response.BadRequest().SetText("'id' is undefined!");

            PhotonServer.Instance.Projects.Remove(id);

            return HttpHandlerResult.Ok();
        }
    }
}
