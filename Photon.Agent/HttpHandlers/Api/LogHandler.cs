using log4net;
using log4net.Appender;
using log4net.Repository.Hierarchy;
using PiServerLite.Http.Handlers;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Photon.Agent.HttpHandlers.Api
{
    [HttpHandler("api/log")]
    internal class LogHandler : HttpHandlerAsync
    {
        public override async Task<HttpHandlerResult> GetAsync()
        {
            var fileAppender = ((Hierarchy)LogManager.GetRepository())
                .Root.Appenders.OfType<FileAppender>().FirstOrDefault();

            if (fileAppender == null)
                return Status(HttpStatusCode.NoContent)
                    .SetText("No file logger was found.");

            var logFile = fileAppender.File;

            if (!File.Exists(logFile))
                return Status(HttpStatusCode.NoContent)
                    .SetText("Log file was not found!");

            var bufferStream = new MemoryStream();

            try {
                using (var fileStream = File.Open(logFile, FileMode.Open, FileAccess.Read)) {
                    await fileStream.CopyToAsync(bufferStream);
                }
            }
            catch {
                bufferStream.Dispose();
                throw;
            }

            return Ok()
                .SetContentType("text/plain")
                .SetContent(bufferStream);
        }
    }
}
