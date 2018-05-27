using System.Reflection;
using PiServerLite.Http.Handlers;

namespace Photon.Server.ApiHandlers
{
    [HttpHandler("api/version")]
    internal class VersionHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version.ToString();

            return Response.Ok().SetText(version);
        }
    }
}
