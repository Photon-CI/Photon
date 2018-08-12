using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;
using System.Reflection;

namespace Photon.Server.ApiHandlers
{
    [Secure]
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
