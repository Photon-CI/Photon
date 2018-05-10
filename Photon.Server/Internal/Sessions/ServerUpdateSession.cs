using Photon.Communication;
using Photon.Framework.Extensions;
using Photon.Framework.Server;
using Photon.Framework.Tools;
using Photon.Library.TcpMessages;
using System;
using System.Linq;
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

        public string[] AgentNames {get; set;}
        public string UpdateFilename {get; set;}
        public string UpdateVersion {get; set;}


        public override async Task RunAsync()
        {
            var agents = PhotonServer.Instance.Agents.All
                .Where(x => IncludesAgent(x.Name)).ToArray();

            if (!agents.Any()) throw new ApplicationException("No agents were found!");

            var queueOptions = new ExecutionDataflowBlockOptions {
                MaxDegreeOfParallelism = Configuration.Parallelism,
                CancellationToken = TokenSource.Token,
            };

            var queue = new ActionBlock<ServerAgent>(AgentAction, queueOptions);

            foreach (var agent in agents)
                queue.Post(agent);

            queue.Complete();
            await queue.Completion;
        }

        protected override DomainAgentSessionHostBase OnCreateHost(ServerAgent agent)
        {
            return null;
        }

        private async Task AgentAction(ServerAgent agent)
        {
            using (var messageClient = new MessageClient(PhotonServer.Instance.MessageRegistry)) {
                Output.Append("Connecting to agent ", ConsoleColor.DarkCyan)
                    .Append(agent.Name, ConsoleColor.Cyan)
                    .AppendLine("...", ConsoleColor.DarkCyan);

                try {
                    await messageClient.ConnectAsync(agent.TcpHost, agent.TcpPort, TokenSource.Token);

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
                }
                catch (Exception error) {
                    Output.Append("Failed to connect to agent ", ConsoleColor.DarkRed)
                        .Append(agent.Name, ConsoleColor.Red)
                        .AppendLine("!", ConsoleColor.DarkRed)
                        .AppendLine(error.UnfoldMessages(), ConsoleColor.DarkYellow);

                    return;
                }

                Output.AppendLine("Agent connected.", ConsoleColor.DarkGreen);

                Output.Append("Updating Agent ", ConsoleColor.DarkCyan)
                    .Append(agent.Name, ConsoleColor.Cyan)
                    .AppendLine("...", ConsoleColor.DarkCyan);

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

        private async Task UpdateAgent(ServerAgent agent, MessageClient messageClient, CancellationToken token)
        {
            var message = new AgentUpdateRequest {
                Filename = UpdateFilename,
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

        private async Task<MessageClient> Reconnect(ServerAgent agent, TimeSpan timeout)
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

                    if (!VersionTools.HasUpdates(versionResponse.Version, UpdateVersion))
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
