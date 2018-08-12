using Photon.Server.Internal;
using Photon.Server.Internal.Security;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;

namespace Photon.Server.ApiHandlers.Variable
{
    [Secure]
    [RequiresRoles(GroupRole.VariablesEdit)]
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
