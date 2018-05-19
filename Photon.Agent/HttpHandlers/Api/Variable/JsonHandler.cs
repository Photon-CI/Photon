using Photon.Agent.Internal;
using Photon.Library.Variables;
using PiServerLite.Http.Handlers;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Agent.HttpHandlers.Api.Variable
{
    [HttpHandler("/api/variable/json")]
    internal class JsonHandler : HttpHandlerAsync
    {
        public override async Task<HttpHandlerResult> GetAsync(CancellationToken token)
        {
            var id = GetQuery("id");

            if (string.IsNullOrEmpty(id))
                return Response.BadRequest().SetText("'id' is undefined!");

            if (!PhotonAgent.Instance.Variables.TryGet(id, out var document))
                return Response.BadRequest().SetText($"Variable Set '{id}' not found!");

            var json = await document.GetJson();

            return Response.Ok()
                .SetHeader("Content-Disposition", $"attachment; filename={document.Id}.json")
                .SetContentType("application/json")
                .SetText(json);
        }

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

            await PhotonAgent.Instance.Variables.Update(document, prevId);

            return Response.Ok();
        }
    }
}
