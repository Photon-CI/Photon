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
                .Where(x => AgentIds.Contains(x.Id, StringComparer.OrdinalIgnoreCase)).ToArray();

            if (!agents.Any()) {
                Output.WriteLine("No agents were found!", ConsoleColor.DarkYellow);
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
                Output.WriteBlock(block => block
                    .Write("Connecting to agent ", ConsoleColor.DarkCyan)
                    .Write(agent.Name, ConsoleColor.Cyan)
                    .WriteLine("...", ConsoleColor.DarkCyan));

                try {
                    await messageClient.ConnectAsync(agent.TcpHost, agent.TcpPort, TokenSource.Token);

                    await ClientHandshake.Verify(messageClient, TokenSource.Token);
                }
                catch (Exception error) {
                    Output.WriteBlock(block => block
                        .Write("Failed to connect to agent ", ConsoleColor.DarkRed)
                        .Write(agent.Name, ConsoleColor.Red)
                        .WriteLine("!", ConsoleColor.DarkRed)
                        .WriteLine(error.UnfoldMessages(), ConsoleColor.DarkYellow));

                    return;
                }

                Output.WriteLine("Agent connected.", ConsoleColor.DarkGreen);

                Output.WriteBlock(block => block
                    .Write("Updating Agent ", ConsoleColor.DarkCyan)
                    .Write(agent.Name, ConsoleColor.Cyan)
                    .WriteLine("...", ConsoleColor.DarkCyan));

                try {
                    await UpdateAgent(agent, messageClient, TokenSource.Token);

                    Output.WriteBlock(block => block
                        .Write("Agent ", ConsoleColor.DarkGreen)
                        .Write(agent.Name, ConsoleColor.Green)
                        .WriteLine(" updated successfully.", ConsoleColor.DarkGreen));
                }
                catch (Exception error) {
                    Output.WriteBlock(block => block
                        .Write("Failed to update agent ", ConsoleColor.DarkRed)
                        .Write(agent.Name, ConsoleColor.Red)
                        .WriteLine("!", ConsoleColor.DarkRed)
                        .WriteLine(error.UnfoldMessages(), ConsoleColor.DarkYellow));
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
                    messageClient.Disconnect();
                }
                catch {}
            }
            finally {
                messageClient?.Dispose();
                messageClient = null;
            }

            Output.WriteBlock(block => block
                .Write("Agent update started on ", ConsoleColor.DarkCyan)
                .Write(agent.Name, ConsoleColor.Cyan)
                .WriteLine("...", ConsoleColor.DarkCyan));

            await Task.Delay(6_000, token);


            var reconnectTimeout = TimeSpan.FromMinutes(2);

            try {
                messageClient = await Reconnect(agent, reconnectTimeout, token);
            }
            catch (OperationCanceledException) {
                throw new ApplicationException($"A timeout occurred after {reconnectTimeout} while waiting for the update to complete.");
            }
            finally {
                if (messageClient != null) {
                    messageClient.Disconnect();
                    messageClient.Dispose();
                }
            }
        }

        private async Task<MessageClient> Reconnect(ServerAgent agent, TimeSpan timeout, CancellationToken token)
        {
            using (var timeoutTokenSource = new CancellationTokenSource(timeout))
            using (var mergedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(timeoutTokenSource.Token, token)) {
                while (true) {
                    mergedTokenSource.Token.ThrowIfCancellationRequested();

                    var client = new MessageClient(PhotonServer.Instance.MessageRegistry);

                    try {
                        using (var connectionTimeoutTokenSource = new CancellationTokenSource(20_000))
                        using (var connectTokenSource = CancellationTokenSource.CreateLinkedTokenSource(mergedTokenSource.Token, connectionTimeoutTokenSource.Token)) {
                            await client.ConnectAsync(agent.TcpHost, agent.TcpPort, connectTokenSource.Token);

                            await ClientHandshake.Verify(client, connectTokenSource.Token);

                            var versionRequest = new AgentGetVersionRequest();

                            var versionResponse = await client.Send(versionRequest)
                                .GetResponseAsync<AgentGetVersionResponse>(connectTokenSource.Token);

                            if (string.IsNullOrEmpty(versionResponse.Version)) {
                                Log.Warn("An empty version response was received!");
                                continue;
                            }

                            if (!VersionTools.HasUpdates(versionResponse.Version, UpdateVersion))
                                return client;
                        }
                    }
                    catch (SocketException) {}
                    catch (OperationCanceledException) {}
                    catch (Exception error) {
                        Log.Warn("An unhandled exception occurred while attempting to reconnect to an updating agent.", error);
                    }

                    client.Dispose();

                    await Task.Delay(3_000, mergedTokenSource.Token);
                }
            }
        }
    }
}
