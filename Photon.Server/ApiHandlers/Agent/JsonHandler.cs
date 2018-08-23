using Newtonsoft.Json;
using Photon.Framework.Server;
using Photon.Library.Extensions;
using Photon.Library.Http;
using Photon.Server.Internal;
using Photon.Server.Internal.Security;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Server.ApiHandlers.Agent
{
    [Secure]
    [RequiresRoles(GroupRole.AgentEdit)]
    [HttpHandler("/api/agent/json")]
    internal class JsonHandler : HttpApiHandlerAsync
    {
        public override async Task<HttpHandlerResult> GetAsync(CancellationToken token)
        {
            var id = GetQuery("id");

            if (string.IsNullOrEmpty(id))
                return Response.BadRequest().SetText("'id' is undefined!");

            if (!PhotonServer.Instance.Agents.TryGet(id, out var agent))
                return Response.BadRequest().SetText($"Agent '{id}' was not found!");

            return await Response.Json(agent)
                .SetHeader("Content-Disposition", $"attachment; filename={id}.json")
                .AsAsync();
        }

        public override async Task<HttpHandlerResult> PostAsync(CancellationToken token)
        {
            var id = GetQuery("id");

            if (string.IsNullOrEmpty(id))
                return Response.BadRequest().SetText("'id' is undefined!");

            var json = await Request.TextAsync();
            var agent = JsonConvert.DeserializeObject<ServerAgent>(json);

            PhotonServer.Instance.Agents.Save(agent, id);

            return Response.Ok();
        }
    }
}
