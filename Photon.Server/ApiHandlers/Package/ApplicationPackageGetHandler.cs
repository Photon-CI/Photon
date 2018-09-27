using Photon.Library.Http;
using Photon.Server.Internal;
using Photon.Server.Internal.Security;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;
using System.IO;

namespace Photon.Server.ApiHandlers.Package
{
    [Secure]
    [RequiresRoles(GroupRole.PackagesView)]
    [HttpHandler("api/applicationPackage/get")]
    internal class ApplicationPackageGetHandler : HttpApiHandler
    {
        public override HttpHandlerResult Get()
        {
            var serverContext = PhotonServer.Instance.Context;

            var qId = GetQuery("id");
            var qVersion = GetQuery("version");

            if (string.IsNullOrEmpty(qId))
                return Response.BadRequest().SetText("'id' is undefined!");

            if (string.IsNullOrEmpty(qVersion))
                return Response.BadRequest().SetText("'version' is undefined!");

            if (!serverContext.ApplicationPackages.TryGet(qId, qVersion, out var filename))
                return Response.BadRequest().SetText($"Package '{qId} @ {qVersion}' not found!");

            var name = Path.GetFileName(filename);

            return Response.File(filename)
                .SetHeader("content-disposition", $"attachment; filename=\"{name}\"");
        }
    }
}
