using Photon.Library.Variables;
using Photon.Server.Internal;
using Photon.Server.Internal.Security;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Server.ApiHandlers.Variable
{
    [Secure]
    [RequiresRoles(GroupRole.VariablesView)]
    [HttpHandler("/api/variable/json")]
    internal class JsonGetHandler : HttpHandlerAsync
    {
        public override async Task<HttpHandlerResult> GetAsync(CancellationToken token)
        {
            var id = GetQuery("id");

            if (string.IsNullOrEmpty(id))
                return Response.BadRequest().SetText("'id' is undefined!");

            if (!PhotonServer.Instance.Variables.TryGet(id, out var document))
                return Response.BadRequest().SetText($"Variable Set '{id}' not found!");

            var json = await document.GetJson();

            return Response.Ok()
                .SetHeader("Content-Disposition", $"attachment; filename={document.Id}.json")
                .SetContentType("application/json")
                .SetText(json);
        }
    }

    [Secure]
    [RequiresRoles(GroupRole.VariablesEdit)]
    [HttpHandler("/api/variable/json-set")]
    internal class JsonPostHandler : HttpHandlerAsync
    {
        public override async Task<HttpHandlerResult> PostAsync(CancellationToken token)
        {
            var id = GetQuery("id");
            var prevId = GetQuery("prevId");
            var json = await Request.TextAsync();

            if (string.IsNullOrEmpty(id))
                return Response.BadRequest().SetText("'id' is undefined!");

            var document = new VariableSetDocument {
                Id = id,
            };

            document.SetJson(json);

            await PhotonServer.Instance.Variables.Update(document, prevId);

            return Response.Ok();
        }
    }
}
