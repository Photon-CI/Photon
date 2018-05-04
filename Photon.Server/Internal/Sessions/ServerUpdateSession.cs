using Photon.Communication;
using Photon.Framework;
using Photon.Framework.Extensions;
using Photon.Framework.Tools;
using Photon.Library;
using Photon.Library.TcpMessages;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Photon.Server.Internal.Sessions
{
    internal class ServerUpdateSession : ServerSessionBase
    {
        private const int HandshakeTimeoutSec = 30;

        private string LatestAgentVersion;
        private string LatestAgentMsiFilename;
        private string LatestAgentMsiUrl;
        private string tempFilename;
        private LazyAsync<string> GetLatestAgentFilename;

        public string[] AgentNames {get; set;}


        public override async Task RunAsync()
        {
            var agents = PhotonServer.Instance.Definition.Definition.Agents
                .Where(x => IncludesAgent(x.Name)).ToArray();

            if (!agents.Any()) throw new ApplicationException("No agents were found!");

            var agentIndex = await DownloadTools.GetLatestAgentIndex();
            LatestAgentVersion = agentIndex.Version;
            LatestAgentMsiFilename = agentIndex.MsiFilename;

            LatestAgentMsiUrl = NetPath.Combine(Configuration.DownloadUrl, "agent", LatestAgentVersion, LatestAgentMsiFilename);

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

            var queueOptions = new ExecutionDataflowBlockOptions {
                MaxDegreeOfParallelism = 8,
                CancellationToken = TokenSource.Token,
            };

            var queue = new ActionBlock<ServerAgentDefinition>(AgentAction, queueOptions);

            foreach (var agent in agents)
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
                string agentVersion;

                try {
                    await messageClient.ConnectAsync(agent.TcpHost, agent.TcpPort, TokenSource.Token);

                    var agentVersionRequest = new AgentGetVersionRequest();

                    var handshakeRequest = new HandshakeRequest {
                        Key = Guid.NewGuid().ToString(),
                        ServerVersion = Configuration.Version,
                    };

                    var timeout = TimeSpan.FromSeconds(HandshakeTimeoutSec);
                    var handshakeResponse = await messageClient.Handshake<HandshakeResponse>(handshakeRequest, timeout, TokenSource.Token);

                    if (!string.Equals(handshakeRequest.Key, handshakeResponse.Key, StringComparison.Ordinal))
                        throw new ApplicationException("Handshake Failed! An invalid key was returned.");

                    if (!handshakeResponse.PasswordMatch)
                        throw new ApplicationException("Handshake Failed! Unauthorized.");

                    var agentVersionResponse = await messageClient.Send(agentVersionRequest)
                        .GetResponseAsync<AgentGetVersionResponse>(TokenSource.Token);

                    agentVersion = agentVersionResponse.Version;
                }
                catch (Exception error) {
                    Output.Append("Failed to connect to agent ", ConsoleColor.DarkRed)
                        .Append(agent.Name, ConsoleColor.Red)
                        .AppendLine("!", ConsoleColor.DarkRed)
                        .AppendLine(error.UnfoldMessages(), ConsoleColor.DarkYellow);

                    return;
                }

                if (!VersionTools.HasUpdates(agentVersion, LatestAgentVersion)) {
                    Output.Append("Agent ", ConsoleColor.DarkBlue)
                        .Append(agent.Name, ConsoleColor.Blue)
                        .AppendLine(" is up-to-date.", ConsoleColor.DarkBlue);

                    return;
                }

                Output.Append("Updating agent ", ConsoleColor.DarkBlue)
                    .Append(agent.Name, ConsoleColor.Blue)
                    .Append(" from version ", ConsoleColor.DarkBlue)
                    .Append(agentVersion, ConsoleColor.Blue)
                    .AppendLine("...", ConsoleColor.DarkBlue);

                try {
                    await UpdateAgent(agent, messageClient, TokenSource.Token);

                    Output.Append("Agent ", ConsoleColor.DarkGreen)
                        .Append(agent.Name, ConsoleColor.Green)
                        .AppendLine(" updated successfully.", ConsoleColor.DarkGreen);
                }
                catch (Exception error) {
                    Output.Append("Failed to update agent ", ConsoleColor.DarkRed)
                        .Append(agent.Name, ConsoleColor.Red)
                        .AppendLine("!", ConsoleColor.DarkRed)
                        .AppendLine(error.UnfoldMessages(), ConsoleColor.DarkYellow);
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
                messageClient = await Reconnect(agent, TimeSpan.FromMinutes(2));
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

                    var handshakeRequest = new HandshakeRequest {
                        Key = Guid.NewGuid().ToString(),
                        ServerVersion = Configuration.Version,
                    };

                    var handshakeTimeout = TimeSpan.FromSeconds(HandshakeTimeoutSec);
                    var handshakeResponse = await client.Handshake<HandshakeResponse>(handshakeRequest, handshakeTimeout, TokenSource.Token);

                    if (!string.Equals(handshakeRequest.Key, handshakeResponse.Key, StringComparison.Ordinal))
                        throw new ApplicationException("Handshake Failed! An invalid key was returned.");

                    if (!handshakeResponse.PasswordMatch)
                        throw new ApplicationException("Handshake Failed! Unauthorized.");

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

        private bool IncludesAgent(string agentName)
        {
            if (!(AgentNames?.Any() ?? false)) return true;

            foreach (var name in AgentNames) {
                var escapedName = Regex.Escape(name)
                    .Replace("\\?", ".")
                    .Replace("\\*", ".*");

                var namePattern = $"^{escapedName}$";
                if (Regex.IsMatch(agentName, namePattern, RegexOptions.IgnoreCase))
                    return true;
            }

            return false;
        }
    }
}
