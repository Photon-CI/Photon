using Photon.Communication;
using Photon.Framework;
using Photon.Framework.Extensions;
using Photon.Framework.Tools;
using Photon.Library;
using Photon.Library.TcpMessages;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Photon.Server.Internal.Sessions
{
    internal class ServerUpdateSession : ServerSessionBase
    {
        private string LatestAgentVersion;
        private string LatestAgentMsiFilename;
        private string LatestAgentMsiUrl;
        private string tempFilename;
        private LazyAsync<string> GetLatestAgentFilename;


        public override async Task RunAsync()
        {
            var agentIndex = await DownloadTools.GetLatestAgentIndex();
            LatestAgentVersion = (string)agentIndex.Version;
            LatestAgentMsiFilename = (string)agentIndex.MsiFilename;

            LatestAgentMsiUrl = NetPath.Combine("http://download.photon.null511.info/agent", LatestAgentVersion, LatestAgentMsiFilename);

            GetLatestAgentFilename = new LazyAsync<string>(async () => {
                tempFilename = Path.GetTempFileName();

                Output.Append("Downloading latest version ", ConsoleColor.DarkCyan)
                    .AppendLine(LatestAgentVersion, ConsoleColor.Cyan);

                using (var webClient = new WebClient()) {
                    await webClient.DownloadFileTaskAsync(LatestAgentMsiUrl, tempFilename);
                }

                Output.AppendLine("Download complete.", ConsoleColor.DarkGreen);

                return tempFilename;
            });

            var queue = new ActionBlock<ServerAgentDefinition>(AgentAction);

            foreach (var agent in PhotonServer.Instance.Definition.Agents)
                queue.Post(agent);

            queue.Complete();
            await queue.Completion;
        }

        protected override DomainAgentSessionHostBase OnCreateHost(ServerAgentDefinition agent)
        {
            return null;
        }

        private async Task AgentAction(ServerAgentDefinition agent)
        {
            using (var messageClient = new MessageClient(PhotonServer.Instance.MessageRegistry)) {
                await messageClient.ConnectAsync(agent.TcpHost, agent.TcpPort, CancellationToken.None);

                var agentVersionRequest = new AgentGetVersionRequest();

                var agentVersionResponse = await messageClient.Send(agentVersionRequest)
                    .GetResponseAsync<AgentGetVersionResponse>(CancellationToken.None);

                if (!VersionTools.HasUpdates(agentVersionResponse.Version, LatestAgentVersion)) {
                    Output.Append("Agent ", ConsoleColor.DarkBlue)
                        .Append(agent.Name, ConsoleColor.Blue)
                        .AppendLine(" is up-to-date.", ConsoleColor.DarkBlue);

                    return;
                }

                Output.Append("Updating agent ", ConsoleColor.DarkBlue)
                    .Append(agent.Name, ConsoleColor.Blue)
                    .AppendLine(" from version ", ConsoleColor.DarkBlue)
                    .Append(agentVersionResponse.Version, ConsoleColor.Blue)
                    .AppendLine("...", ConsoleColor.DarkBlue);

                try {
                    await UpdateAgent(agent, messageClient, CancellationToken.None);

                    Output.Append("Agent ", ConsoleColor.DarkGreen)
                        .Append(agent.Name, ConsoleColor.Green)
                        .AppendLine(" updated successfully.", ConsoleColor.DarkGreen);
                }
                catch (Exception error) {
                    Output.Append("Failed to update agent ", ConsoleColor.DarkRed)
                        .Append(agent.Name, ConsoleColor.Red)
                        .AppendLine("!", ConsoleColor.DarkRed)
                        .AppendLine(error.UnfoldMessages(), ConsoleColor.DarkYellow);

                    throw;
                }
            }
        }

        private async Task UpdateAgent(ServerAgentDefinition agent, MessageClient messageClient, CancellationToken token)
        {
            var filename = await GetLatestAgentFilename;

            var message = new AgentUpdateRequest {
                Filename = filename,
            };

            try {
                await messageClient.Send(message)
                    .GetResponseAsync(token);

                await messageClient.DisconnectAsync();
            }
            finally {
                messageClient?.Dispose();
            }

            await Task.Delay(3000, token);

            // TODO: Verify update was successful by polling for server and checking version
            messageClient = null;

            try {
                messageClient = await Reconnect(agent, TimeSpan.FromSeconds(60));

                await messageClient.DisconnectAsync();
            }
            finally {
                messageClient?.Dispose();
            }
        }

        private async Task<MessageClient> Reconnect(ServerAgentDefinition agent, TimeSpan timeout)
        {
            var client = new MessageClient(PhotonServer.Instance.MessageRegistry);

            client.ThreadException += (o, e) => {
                var error = (Exception) e.ExceptionObject;
                Output.AppendLine("An error occurred while messaging the client!", ConsoleColor.DarkRed)
                    .AppendLine(error.UnfoldMessages());

                Log.Error("Message Client error after update!", error);
            };

            var tokenSource = new CancellationTokenSource(timeout);

            var token = tokenSource.Token;
            while (true) {
                token.ThrowIfCancellationRequested();

                try {
                    await client.ConnectAsync(agent.TcpHost, agent.TcpPort, tokenSource.Token);

                    var versionRequest = new AgentGetVersionRequest();

                    var versionResponse = await client.Send(versionRequest)
                        .GetResponseAsync<AgentGetVersionResponse>(token);

                    if (!VersionTools.HasUpdates(versionResponse.Version, LatestAgentVersion))
                        break;
                }
                catch (SocketException) {
                    await Task.Delay(1000, tokenSource.Token);
                }
            }

            return client;
        }
    }
}
