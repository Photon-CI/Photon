using Photon.Communication;
using Photon.Framework;
using Photon.Framework.Server;
using Photon.Library.TcpMessages;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Server.Internal
{
    internal class AgentUpdater
    {
        public MessageClient MessageClient {get; set;}
        public string MsiFilename {get; set;}
        public ScriptOutput Output {get; set;}


        public async Task Update(ServerAgentDefinition agent, CancellationToken token)
        {
            //Output
            //    .Append("Checking Agent ", ConsoleColor.DarkCyan)
            //    .Append(agent.Name, ConsoleColor.Cyan)
            //    .AppendLine(" for updates...", ConsoleColor.DarkCyan);

            //MessageClient client = null;

            try {
                //client = new MessageClient(PhotonServer.Instance.MessageRegistry) {
                //    //Context = sessionBase,
                //};

                //MessageClient.ThreadException += MessageClient_OnThreadException;

                //await client.ConnectAsync(agent.TcpHost, agent.TcpPort, token);

                //var versionRequest = new AgentGetVersionRequest();

                //var versionResponse = await client.Send(versionRequest)
                //    .GetResponseAsync<AgentGetVersionResponse>(token);

                //if (!HasUpdates(versionResponse.Version, UpdateVersion)) {
                //    Output
                //        .Append("Agent ", ConsoleColor.DarkBlue)
                //        .Append(agent.Name, ConsoleColor.Blue)
                //        .AppendLine(" is up-to-date. Version: ", ConsoleColor.DarkBlue)
                //        .AppendLine(versionResponse.Version, ConsoleColor.Blue);

                //    return;
                //}

                //Output
                //    .Append("Updating Agent ", ConsoleColor.DarkCyan)
                //    .Append(agent.Name, ConsoleColor.Cyan)
                //    .AppendLine("...", ConsoleColor.DarkCyan);

                var message = new AgentUpdateRequest {
                    Filename = MsiFilename,
                };

                await MessageClient.Send(message)
                    .GetResponseAsync(token);

                await MessageClient.DisconnectAsync();
            }
            finally {
                MessageClient?.Dispose();
            }

            await Task.Delay(3000, token);

            // TODO: Verify update was successful by polling for server and checking version
            client = null;

            try {
                client = await Reconnect(agent, TimeSpan.FromSeconds(60));

                await client.DisconnectAsync();
            }
            finally {
                client?.Dispose();
            }

            Output
                .Append("Agent ", ConsoleColor.DarkGreen)
                .Append(agent.Name, ConsoleColor.Green)
                .AppendLine(" updated successfully.", ConsoleColor.DarkGreen);
        }

        private async Task<MessageClient> Reconnect(ServerAgentDefinition agent, TimeSpan timeout)
        {
            var client = new MessageClient(PhotonServer.Instance.MessageRegistry) {
                //Context = sessionBase,
            };

            //MessageClient.ThreadException += MessageClient_OnThreadException;
            var tokenSource = new CancellationTokenSource(timeout);

            while (!tokenSource.IsCancellationRequested) {
                try {
                    await client.ConnectAsync(agent.TcpHost, agent.TcpPort, tokenSource.Token);
                }
                catch (SocketException) {
                    await Task.Delay(1000, tokenSource.Token);
                    continue;
                }

                var versionRequest = new AgentGetVersionRequest();

                var versionResponse = await client.Send(versionRequest)
                    .GetResponseAsync<AgentGetVersionResponse>(tokenSource.Token);

                if (versionResponse.Version == UpdateVersion) return client;
            }

            client.Dispose();
            return null;
        }

        private static bool HasUpdates(string currentVersion, string newVersion)
        {
            var _current = new Version(currentVersion);
            var _new = new Version(newVersion);

            return _new > _current;
        }
    }
}
