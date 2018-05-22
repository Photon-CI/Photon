using Photon.Communication;
using Photon.Framework.Extensions;
using Photon.Framework.Server;
using Photon.Framework.Tools;
using Photon.Library.TcpMessages;
using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Photon.Server.Internal.Sessions
{
    internal class ServerUpdateSession : ServerSessionBase
    {
        public string[] AgentIds {get; set;}
        public string UpdateFilename {get; set;}
        public string UpdateVersion {get; set;}


        public override async Task RunAsync()
        {
            var agents = PhotonServer.Instance.Agents.All
                .Where(x => AgentIds.Any(id => string.Equals(id, x.Id, StringComparison.OrdinalIgnoreCase))).ToArray();

            if (!agents.Any()) {
                Output.AppendLine("No agents were found!", ConsoleColor.DarkYellow);
                throw new ApplicationException("No agents were found!");
            }

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

                    await ClientHandshake.Verify(messageClient, TokenSource.Token);
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

                try {
                    await messageClient.DisconnectAsync();
                }
                catch {}
            }
            finally {
                messageClient?.Dispose();
            }

            Output.Append("Agent update start on ", ConsoleColor.DarkCyan)
                .Append(agent.Name, ConsoleColor.Cyan)
                .AppendLine("...", ConsoleColor.DarkCyan);

            await Task.Delay(3000, token);

            // TODO: Verify update was successful by polling for server and checking version
            messageClient = null;

            var reconnectTimeout = TimeSpan.FromMinutes(2);

            try {
                messageClient = await Reconnect(agent, reconnectTimeout, token);
            }
            catch (OperationCanceledException) {
                // TODO: Better error messages
                throw new ApplicationException($"A timeout occurred after {reconnectTimeout} while waiting for the update to complete.");
            }
            finally {
                messageClient?.Dispose();
            }
        }

        private async Task<MessageClient> Reconnect(ServerAgent agent, TimeSpan timeout, CancellationToken token)
        {
            var client = new MessageClient(PhotonServer.Instance.MessageRegistry);

            //client.ThreadException += (o, e) => {
            //    var error = (Exception) e.ExceptionObject;
            //    Output.AppendLine("An error occurred while messaging the client!", ConsoleColor.DarkRed)
            //        .AppendLine(error.UnfoldMessages());

            //    Log.Error("Message Client error after update!", error);
            //};

            using (var timeoutTokenSource = new CancellationTokenSource(timeout))
            using (var mergedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(timeoutTokenSource.Token, token)) {
                var _token = mergedTokenSource.Token;

                while (true) {
                    _token.ThrowIfCancellationRequested();

                    try {
                        await client.ConnectAsync(agent.TcpHost, agent.TcpPort, _token);

                        await ClientHandshake.Verify(client, _token);

                        var versionRequest = new AgentGetVersionRequest();

                        var versionResponse = await client.Send(versionRequest)
                            .GetResponseAsync<AgentGetVersionResponse>(_token);

                        if (!VersionTools.HasUpdates(versionResponse.Version, UpdateVersion))
                            break;
                    }
                    catch (SocketException) {
                        await Task.Delay(1000, _token);
                    }
                }
            }

            return client;
        }
    }
}
