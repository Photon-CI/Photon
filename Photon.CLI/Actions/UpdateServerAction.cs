using Photon.CLI.Internal;
using Photon.CLI.Internal.Http;
using Photon.Framework;
using Photon.Framework.Tools;
using Photon.Library;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Photon.Library.Http.Messages;

namespace Photon.CLI.Actions
{
    internal class UpdateServerAction : ActionBase
    {
        public string ServerName {get; set;}


        public async Task Run(CommandContext context)
        {
            var server = context.Servers.Get(ServerName);

            ConsoleEx.Out.WriteLine("Checking server version...", ConsoleColor.DarkCyan);

            string currentVersion = null;
            await HttpAuthAsync(async () => {
                currentVersion = await WebClient(server, async client => {
                    return (await client.DownloadStringTaskAsync("api/version")).Trim();
                });
            });

            ConsoleEx.Out
                .WriteLine($"Photon Server {currentVersion}", ConsoleColor.DarkBlue)
                .WriteLine("Checking for updates...", ConsoleColor.DarkCyan);

            var serverIndex = await DownloadTools.GetLatestServerIndex();
            
            if (!VersionTools.HasUpdates(currentVersion, serverIndex.Version)) {
                ConsoleEx.Out.WriteLine("Server is up-to-date.", ConsoleColor.DarkBlue);

                return;
            }

            await BeginServerUpdate(server, serverIndex);

            ConsoleEx.Out.WriteLine("Server update started. Waiting for restart...", ConsoleColor.Cyan);

            await Task.Delay(3000);

            var timeout = TimeSpan.FromMinutes(2);

            try {
                await Reconnect(server, serverIndex.Version, timeout);

                ConsoleEx.Out.WriteLine("Update completed successfully.", ConsoleColor.DarkGreen);
            }
            catch (TaskCanceledException) {
                throw new ApplicationException($"Server failed to restart within timeout '{timeout}'.");
            }
        }

        private async Task BeginServerUpdate(PhotonServerDefinition server, HttpPackageIndex index)
        {
            ConsoleEx.Out.Write("Downloading Server update ", ConsoleColor.DarkCyan)
                .Write(index.Version, ConsoleColor.Cyan)
                .WriteLine("...", ConsoleColor.DarkCyan);

            var updateDirectory = Path.Combine(Configuration.Directory, "Updates");
            var updateFilename = Path.Combine(updateDirectory, "Photon.Server.msi");

            PathEx.CreatePath(updateDirectory);

            using (var client = new WebClient()) {
                var url = NetPath.Combine(Configuration.DownloadUrl, "server", index.Version, index.MsiFilename);
                await client.DownloadFileTaskAsync(url, updateFilename);
            }

            ConsoleEx.Out
                .WriteLine("Download Complete.", ConsoleColor.DarkBlue)
                .WriteLine("Uploading update to Server...", ConsoleColor.DarkCyan);

            await WebClientEx(server, client => {
                client.Method = "POST";
                client.Url = NetPath.Combine(server.Url, "api/server/update");
                client.ContentType = "application/octet-stream";
                client.BodyFunc = () => File.Open(updateFilename, FileMode.Open, FileAccess.Read);
            }, null);

            ConsoleEx.Out.WriteLine("Upload Complete.", ConsoleColor.DarkBlue);
        }

        private async Task Reconnect(PhotonServerDefinition server, string latestVersion, TimeSpan timeout)
        {
            using (var tokenSource = new CancellationTokenSource(timeout)) 
            using (var webClient = WebClientFactory.Create(server, Username, Password)) {
                var token = tokenSource.Token;
                while (true) {
                    token.ThrowIfCancellationRequested();

                    try {
                        var version = await webClient.DownloadStringTaskAsync("api/version");

                        if (!VersionTools.HasUpdates(version, latestVersion)) break;
                    }
                    catch (Exception error) when (error is SocketException || error is WebException) {
                        await Task.Delay(1000, tokenSource.Token);
                    }
                }
            }
        }
    }
}
