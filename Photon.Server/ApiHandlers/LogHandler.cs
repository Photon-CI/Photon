using log4net;
using log4net.Appender;
using log4net.Repository.Hierarchy;
using PiServerLite.Http.Handlers;
using PiServerLite.Http.Security;
using System.IO;
using System.Linq;
using System.Net;

namespace Photon.Server.ApiHandlers
{
    [Secure]
    [HttpHandler("api/log")]
    internal class LogHandler : HttpHandler
    {
        public override HttpHandlerResult Get()
        {
            var fileAppender = ((Hierarchy)LogManager.GetRepository())
                .Root.Appenders.OfType<FileAppender>().FirstOrDefault();

            if (fileAppender == null)
                return Response.Status(HttpStatusCode.NoContent)
                    .SetText("No file logger was found.");

            var logFile = fileAppender.File;

            if (!File.Exists(logFile))
                return Response.Status(HttpStatusCode.NoContent)
                    .SetText("Log file was not found!");

            return Response.File(logFile);
        }
    }
}
