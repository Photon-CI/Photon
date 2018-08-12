using Photon.Server.Internal;
using Photon.Server.Internal.Security;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;

namespace Photon.Server.ApiHandlers.Project
{
    [Secure]
    [RequiresRoles(GroupRole.ProjectEdit)]
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
