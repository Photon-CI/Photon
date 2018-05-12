using PiServerLite.Http.Handlers;
using System.Reflection;

namespace Photon.Server.HttpHandlers.Api
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
