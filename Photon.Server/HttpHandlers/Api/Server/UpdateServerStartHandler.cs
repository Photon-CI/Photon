using log4net;
using Photon.Framework;
using Photon.Framework.Tools;
using Photon.Server.Internal;
using PiServerLite.Http.Handlers;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Server.HttpHandlers.Api.Server
{
    [HttpHandler("api/server/update")]
    internal class UpdateServerStartHandler : HttpHandlerAsync
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(UpdateServerStartHandler));


        public override async Task<HttpHandlerResult> PostAsync(CancellationToken token)
        {
            if (HttpContext.Request.ContentLength64 == 0)
                return Response.BadRequest().SetText("The request was empty!");

            try {
                var updatePath = Path.Combine(Configuration.Directory, "Updates");
                var msiFilename = Path.Combine(updatePath, "Photon.Server.msi");

                PathEx.CreatePath(updatePath);

                using (var fileStream = File.Open(msiFilename, FileMode.Create, FileAccess.Write)) {
                    await HttpContext.Request.InputStream.CopyToAsync(fileStream);
                }

                var _ = Task.Delay(100, token)
                    .ContinueWith(t => {
                        BeginInstall(updatePath, msiFilename);
                    }, token);

                return Response.Ok().SetText("Shutting down and performing update...");
            }
            catch (Exception error) {
                Log.Error("Failed to run Update-Task!", error);
                return Response.Exception(error);
            }
        }

        private async void BeginInstall(string updatePath, string msiFilename)
        {
            // TODO: Verify MSI?

            try {
                await PhotonServer.Instance.Shutdown(TimeSpan.FromSeconds(30));
            }
            catch (Exception error) {
                Log.Error("An error occurred while shutting down!", error);
            }

            try {
                var cmd = $"msiexec.exe /i \"{msiFilename}\" /passive /l*vx \"log.txt\"";

                ProcessRunner.Run(updatePath, cmd);
            }
            catch (Exception error) {
                Log.Error("Failed to start server update!", error);
            }
        }
    }
}
