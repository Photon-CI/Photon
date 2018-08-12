using Photon.CLI.Internal;
using Photon.CLI.Internal.Http;
using Photon.Framework;
using Photon.Framework.Tools;
using Photon.Library;
using Photon.Library.HttpMessages;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.CLI.Actions
{
    internal class UpdateServerAction
    {
        public string ServerName {get; set;}
        public string Username {get; set;}
        public string Password {get; set;}


        public async Task Run(CommandContext context)
        {
            var server = context.Servers.Get(ServerName);

            ConsoleEx.Out.WriteLine("Checking server version...", ConsoleColor.DarkCyan);

            string currentVersion;
            using (var webClient = new WebClient()) {
                if (!string.IsNullOrEmpty(Username)) {
                    var creds = Encoding.ASCII.GetBytes($"{Username}:{Password}");
                    var creds64 = Convert.ToBase64String(creds);
                    webClient.Headers[HttpRequestHeader.Authorization] = $"Basic {creds64}";
                }

                var currentVersionUrl = NetPath.Combine(server.Url, "api/version");
                currentVersion = (await webClient.DownloadStringTaskAsync(currentVersionUrl)).Trim();
            }

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

            string updateFilename;

            try {
                var url = NetPath.Combine(Configuration.DownloadUrl, "server", index.Version, index.MsiFilename);

                var updateDirectory = Path.Combine(Configuration.Directory, "Updates");
                updateFilename = Path.Combine(updateDirectory, "Photon.Server.msi");

                PathEx.CreatePath(updateDirectory);

                using (var client = HttpClientEx.Get(url)) {
                    await client.Send();

                    using (var responseStream = client.ResponseBase.GetResponseStream()) {
                    using (var fileStream = File.Open(updateFilename, FileMode.Create, FileAccess.Write))
                        if (responseStream != null)
                            await responseStream.CopyToAsync(fileStream);
                    }
                }

                ConsoleEx.Out.WriteLine("Download Complete.", ConsoleColor.DarkBlue);
            }
            catch (HttpStatusCodeException error) {
                if (error.HttpCode == HttpStatusCode.NotFound)
                    throw new ApplicationException($"Photon-Server instance '{server.Name}' not found!");

                throw;
            }

            ConsoleEx.Out.WriteLine("Uploading update to Server...", ConsoleColor.DarkCyan);

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

                ConsoleEx.Out.WriteLine("Upload Complete.", ConsoleColor.DarkBlue);
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
