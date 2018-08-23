using Photon.CLI.Internal;
using Photon.CLI.Internal.Http;
using Photon.Framework;
using Photon.Framework.Tools;
using Photon.Library;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Photon.Library.Http.Messages;

namespace Photon.CLI.Actions
{
    internal class UpdateSelfAction
    {
        private string updateDirectory;
        private string updateFilename;


        public async Task Run(CommandContext context)
        {
            ConsoleEx.Out
                .WriteLine($"Photon CLI {Configuration.Version}", ConsoleColor.DarkBlue)
                .WriteLine("Checking for updates...", ConsoleColor.DarkCyan);

            var index = await DownloadTools.GetLatestCliIndex();
            var latestVersion = index.Version;
            
            if (!VersionTools.HasUpdates(Configuration.Version, latestVersion)) {
                ConsoleEx.Out.WriteLine("CLI is up-to-date.", ConsoleColor.DarkBlue);
                return;
            }

            ConsoleEx.Out.Write("Downloading CLI update ", ConsoleColor.DarkCyan)
                .Write(latestVersion, ConsoleColor.Cyan)
                .WriteLine("...", ConsoleColor.DarkCyan);

            updateDirectory = Path.Combine(Configuration.Directory, "Updates");
            updateFilename = Path.Combine(updateDirectory, "Photon.Server.msi");

            await DownloadUpdate(index);

            ConsoleEx.Out
                .WriteLine("Download Complete.", ConsoleColor.DarkGreen)
                .WriteLine("Launching installer...", ConsoleColor.DarkCyan);

            StartInstaller();
        }

        private async Task DownloadUpdate(HttpPackageIndex index)
        {
            PathEx.CreatePath(updateDirectory);

            try {
                var url = NetPath.Combine(Configuration.DownloadUrl, "CLI", index.Version, index.MsiFilename);

                using (var client = HttpClientEx.Get(url)) {
                    await client.Send();

                    using (var fileStream = File.Open(updateFilename, FileMode.Create, FileAccess.Write))
                    using (var responseStream = client.ResponseBase.GetResponseStream()) {
                        if (responseStream != null)
                            await responseStream.CopyToAsync(fileStream);
                    }
                }
            }
            catch (HttpStatusCodeException error) {
                throw new ApplicationException("Failed to download CLI update!", error);
            }
        }

        private void StartInstaller()
        {
            var info = new ProcessStartInfo {
                FileName = "msiexec.exe",
                Arguments = $"/i \"{updateFilename}\" /passive /l*vx \"log.txt\"",
            };

            using (var _ = Process.Start(info)) {}
        }
    }
}
