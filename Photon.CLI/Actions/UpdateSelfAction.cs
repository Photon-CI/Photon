using Photon.CLI.Internal;
using Photon.CLI.Internal.Http;
using Photon.Framework;
using Photon.Framework.Tools;
using Photon.Library;
using Photon.Library.HttpMessages;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Photon.CLI.Actions
{
    internal class UpdateSelfAction
    {
        private string updateDirectory;
        private string updateFilename;


        public async Task Run(CommandContext context)
        {
            var index = await DownloadTools.GetLatestCliIndex();
            var latestVersion = index.Version;
            
            if (!VersionTools.HasUpdates(Configuration.Version, latestVersion)) {
                ConsoleEx.Out
                    .Write("CLI is up-to-date. Version ", ConsoleColor.DarkCyan)
                    .Write(Configuration.Version, ConsoleColor.Cyan)
                    .WriteLine(".", ConsoleColor.DarkCyan);

                return;
            }

            ConsoleEx.Out.WriteLine("Downloading CLI update...", ConsoleColor.DarkCyan);

            updateDirectory = Path.Combine(Configuration.Directory, "Updates");
            updateFilename = Path.Combine(updateDirectory, "Photon.Server.msi");

            await DownloadUpdate(index);

            ConsoleEx.Out
                .WriteLine("Downloading Complete.", ConsoleColor.DarkGreen)
                .WriteLine("Launching installer...", ConsoleColor.DarkCyan);

            StartInstaller();
        }

        private async Task DownloadUpdate(HttpPackageIndex index)
        {
            if (!Directory.Exists(updateDirectory))
                Directory.CreateDirectory(updateDirectory);

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
                Arguments = $"/I \"{updateFilename}\" /passive /fe /L*V \"log.txt\"",
            };

            using (var _ = Process.Start(info)) {}
        }
    }
}
