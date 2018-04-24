using Photon.CLI.Internal;
using Photon.CLI.Internal.Http;
using Photon.Framework;
using Photon.Framework.Tools;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Photon.Library;

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
            var latestServerVersion = (string)serverIndex.Version;
            
            if (!VersionTools.HasUpdates(currentVersion, latestServerVersion)) {
                ConsoleEx.Out
                    .Write("Server is up-to-date. Version ", ConsoleColor.DarkCyan)
                    .Write(currentVersion, ConsoleColor.Cyan)
                    .WriteLine(".", ConsoleColor.DarkCyan);

                return;
            }

            ConsoleEx.Out.WriteLine("Downloading server update...", ConsoleColor.DarkCyan);

            await BeginServerUpdate(server);

            ConsoleEx.Out
                .WriteLine("Server update started.", ConsoleColor.Cyan)
                .WriteLine("Waiting for restart...", ConsoleColor.DarkCyan);

            await Task.Delay(3000);

            await Reconnect(server, latestServerVersion, TimeSpan.FromMinutes(2));
        }

        private async Task BeginServerUpdate(PhotonServerDefinition server)
        {
            HttpClientEx client = null;

            try {
                var url = NetPath.Combine(server.Url, "api/server/update/start");

                client = HttpClientEx.Post(url);

                await client.Send();

                if (client.ResponseBase.StatusCode == HttpStatusCode.BadRequest) {
                    var text = await client.GetResponseTextAsync();
                    throw new ApplicationException($"Bad Update Request! {text}");
                }
            }
            catch (HttpStatusCodeException error) {
                if (error.HttpCode == HttpStatusCode.NotFound)
                    throw new ApplicationException($"Photon-Server instance '{server.Name}' not found!");

                throw;
            }
            finally {
                client?.Dispose();
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
                    catch (SocketException) {
                        await Task.Delay(1000, tokenSource.Token);
                    }
                }
            }
        }
    }
}
