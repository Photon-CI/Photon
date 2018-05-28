using System.IO;
using System.Linq;
using System.Net;
using log4net;
using log4net.Appender;
using log4net.Repository.Hierarchy;
using PiServerLite.Http.Handlers;

namespace Photon.Agent.ApiHandlers
{
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
