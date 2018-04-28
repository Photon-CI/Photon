using Photon.CLI.Internal;
using Photon.CLI.Internal.Http;
using Photon.Framework;
using Photon.Framework.Tools;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Photon.Library;
using Photon.Library.HttpMessages;

namespace Photon.CLI.Actions
{
    internal class UpdateServerAction
    {
        public string ServerName {get; set;}


        public async Task Run(CommandContext context)
        {
            var server = context.Servers.Get(ServerName);

            string currentVersion;
            using (var webClient = new WebClient()) {
                var currentVersionUrl = NetPath.Combine(server.Url, "api/version");
                currentVersion = (await webClient.DownloadStringTaskAsync(currentVersionUrl)).Trim();
            }

            var serverIndex = await DownloadTools.GetLatestServerIndex();
            var latestServerVersion = serverIndex.Version;
            
            if (!VersionTools.HasUpdates(currentVersion, latestServerVersion)) {
                ConsoleEx.Out
                    .Write("Server is up-to-date. Version ", ConsoleColor.DarkCyan)
                    .Write(currentVersion, ConsoleColor.Cyan)
                    .WriteLine(".", ConsoleColor.DarkCyan);

                return;
            }

            ConsoleEx.Out.WriteLine("Downloading server update...", ConsoleColor.DarkCyan);

            await BeginServerUpdate(server, serverIndex);

            ConsoleEx.Out
                .WriteLine("Server update started.", ConsoleColor.Cyan)
                .WriteLine("Waiting for restart...", ConsoleColor.DarkCyan);

            await Task.Delay(3000);

            var timeout = TimeSpan.FromMinutes(2);

            try {
                await Reconnect(server, latestServerVersion, timeout);
            }
            catch (TaskCanceledException) {
                throw new ApplicationException($"Server failed to restart within timeout '{timeout}'.");
            }
        }

        private async Task BeginServerUpdate(PhotonServerDefinition server, HttpPackageIndex index)
        {
            string updateFilename;

            try {
                var url = NetPath.Combine(Configuration.DownloadUrl, "server", index.Version, index.MsiFilename);

                using (var client = HttpClientEx.Get(url)) {
                    await client.Send();

                    var updateDirectory = Path.Combine(Configuration.Directory, "Updates");
                    updateFilename = Path.Combine(updateDirectory, "Photon.Server.msi");

                    if (!Directory.Exists(updateDirectory))
                        Directory.CreateDirectory(updateDirectory);

                    using (var fileStream = File.Open(updateFilename, FileMode.Create, FileAccess.Write))
                    using (var responseStream = client.ResponseBase.GetResponseStream()) {
                        if (responseStream != null)
                            await responseStream.CopyToAsync(fileStream);
                    }
                }
            }
            catch (HttpStatusCodeException error) {
                if (error.HttpCode == HttpStatusCode.NotFound)
                    throw new ApplicationException($"Photon-Server instance '{server.Name}' not found!");

                throw;
            }

            try {
                var url = NetPath.Combine(server.Url, "api/server/update");

                using (var client = HttpClientEx.Post(url)) {
                    client.ContentType = "application/octet-stream";
                    client.BodyFunc = () => File.Open(updateFilename, FileMode.Open, FileAccess.Read);

                    await client.Send();

                    if (client.ResponseBase.StatusCode == HttpStatusCode.BadRequest) {
                        var text = await client.GetResponseTextAsync();
                        throw new ApplicationException($"Bad Update Request! {text}");
                    }
                }
            }
            catch (HttpStatusCodeException error) {
                if (error.HttpCode == HttpStatusCode.NotFound)
                    throw new ApplicationException($"Photon-Server instance '{server.Name}' not found!");

                throw;
            }
        }

        private async Task Reconnect(PhotonServerDefinition server, string latestVersion, TimeSpan timeout)
        {
            using (var tokenSource = new CancellationTokenSource(timeout)) 
            using (var client = new WebClient()) {
                var token = tokenSource.Token;
                while (true) {
                    token.ThrowIfCancellationRequested();

                    try {
                        var url = NetPath.Combine(server.Url, "api/version");
                        var version = await client.DownloadStringTaskAsync(url);

                        if (!VersionTools.HasUpdates(version, latestVersion))
                            break;
                    }
                    catch (Exception error) when (error is SocketException || error is WebException) {
                        await Task.Delay(1000, tokenSource.Token);
                    }
                }
            }
        }
    }
}
