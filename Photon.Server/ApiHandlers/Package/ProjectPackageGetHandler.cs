using Photon.Server.Internal;
using PiServerLite.Http.Handlers;
using System.IO;

namespace Photon.Server.ApiHandlers.Package
{
    [HttpHandler("api/projectPackage/get")]
    internal class ProjectPackageGetHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var qId = GetQuery("id");
            var qVersion = GetQuery("version");

            if (string.IsNullOrEmpty(qId))
                return Response.BadRequest().SetText("'id' is undefined!");

            if (string.IsNullOrEmpty(qVersion))
                return Response.BadRequest().SetText("'version' is undefined!");

            if (!PhotonServer.Instance.ProjectPackages.TryGet(qId, qVersion, out var filename))
                return Response.BadRequest().SetText($"Package '{qId} @ {qVersion}' not found!");

            var name = Path.GetFileName(filename);

            return Response.File(filename)
                .SetHeader("content-disposition", $"attachment; filename=\"{name}\"");
        }
    }
}
