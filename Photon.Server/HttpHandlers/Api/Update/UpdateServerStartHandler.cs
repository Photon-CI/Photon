﻿using log4net;
using Photon.Framework;
using Photon.Server.Internal;
using PiServerLite.Http.Handlers;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace Photon.Server.HttpHandlers.Api.Update
{
    [HttpHandler("/api/update/server/start")]
    internal class UpdateServerStartHandler : HttpHandlerAsync
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(UpdateServerStartHandler));


        public override async Task<HttpHandlerResult> PostAsync()
        {
            if (HttpContext.Request.ContentLength64 == 0)
                return BadRequest().SetText("The request was empty!");

            try {
                var updatePath = Path.Combine(Configuration.Directory, "Updates");
                var msiFilename = Path.Combine(updatePath, "Photon.Server.msi");
                var configFilename = Path.Combine(updatePath, "server.json");

                if (HttpContext.Request.ContentType == "application/zip") {
                    using (var archive = new ZipArchive(HttpContext.Request.InputStream)) {
                        var msiEntry = archive.Entries.FirstOrDefault(x => {
                            var ext = Path.GetExtension(x.Name);
                            return string.Equals(".msi", ext, StringComparison.OrdinalIgnoreCase);
                        });

                        if (msiEntry == null)
                            return BadRequest().SetText("No MSI file was found in the archive!");

                        using (var entryStream = msiEntry.Open()) 
                        using (var fileStream = File.Open(msiFilename, FileMode.Create, FileAccess.Write)) {
                            await entryStream.CopyToAsync(fileStream);
                        }

                        var configEntry = archive.Entries.FirstOrDefault(x =>
                            string.Equals("server.json", x.Name, StringComparison.OrdinalIgnoreCase));

                        if (configEntry != null) {
                            using (var entryStream = configEntry.Open()) 
                            using (var fileStream = File.Open(configFilename, FileMode.Create, FileAccess.Write)) {
                                await entryStream.CopyToAsync(fileStream);
                            }
                        }
                    }
                }
                else if (HttpContext.Request.ContentType == "application/octet-stream") {
                    using (var fileStream = File.Open(msiFilename, FileMode.Create, FileAccess.Write)) {
                        await HttpContext.Request.InputStream.CopyToAsync(fileStream);
                    }
                }
                else {
                    return BadRequest().SetText($"Invalid content-type! '{HttpContext.Request.ContentType}'");
                }

                // TODO: Verify MSI?

                try {
                    await PhotonServer.Instance.Shutdown(TimeSpan.FromSeconds(30));
                }
                catch (Exception error) {
                    Log.Error("An error occurred while shutting down!", error);
                }

                try {
                    var cmd = $"msiexec.exe /i \"{msiFilename}\" /quiet /passive /qn /L*V \"log.txt\"";

                    ProcessRunner.Run(updatePath, cmd);
                }
                catch (Exception error) {
                    Log.Error("Failed to start server update!", error);
                    return Exception(new ApplicationException("Failed to start server update!", error));
                }

                return Ok().SetText("Shutting down and performing update...");
            }
            catch (Exception error) {
                Log.Error("Failed to run Update-Task!", error);
                return Exception(error);
            }
        }
    }
}
