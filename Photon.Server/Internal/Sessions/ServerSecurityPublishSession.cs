using Photon.Communication;
using Photon.Framework.Extensions;
using Photon.Framework.Server;
using Photon.Library.TcpMessages.Security;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Photon.Server.Internal.Sessions
{
    internal class ServerSecurityPublishSession : ServerSessionBase
    {
        //public string[] AgentIds {get; set;}


        public override async Task RunAsync()
        {
            var agents = PhotonServer.Instance.Agents.All
                .OrderBy(x => x.Name).ToArray();
                //.Where(x => AgentIds.Contains(x.Id, StringComparer.OrdinalIgnoreCase)).ToArray();

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
                try {
                    Output.WriteLine($"[{agent.Name}] Connecting...'", ConsoleColor.White);

                    await messageClient.ConnectAsync(agent.TcpHost, agent.TcpPort, TokenSource.Token);

                    await ClientHandshake.Verify(messageClient, TokenSource.Token);

                    Output.WriteLine($"[{agent.Name}] Connected.", ConsoleColor.DarkCyan);
                }
                catch (Exception error) {
                    using (var block = Output.WriteBlock()) {
                        block.WriteLine($"[{agent.Name}] Connection Failed!", ConsoleColor.DarkRed);
                        block.WriteLine(error.UnfoldMessages(), ConsoleColor.DarkYellow);
                    }

                    return;
                }

                var userMgr = PhotonServer.Instance.UserMgr;
                var securityConfig = PhotonServer.Instance.ServerConfiguration.Value.Security;

                var message = new SecurityPublishRequest {
                    Users = userMgr.AllUsers.ToArray(),
                    UserGroups = userMgr.AllGroups.ToArray(),
                    SecurityEnabled = securityConfig.Enabled,
                    SecurityDomainEnabled = securityConfig.DomainEnabled,
                };

                Output.WriteLine($"[{agent.Name}] Publishing security configuration...", ConsoleColor.White);

                await messageClient.Send(message)
                    .GetResponseAsync();

                Output.WriteLine($"[{agent.Name}] security configuration published.", ConsoleColor.DarkGreen);

                try {
                    Output.WriteLine($"[{agent.Name}] Disconnecting...", ConsoleColor.White);

                    messageClient.Disconnect();
                }
                catch {}
            }
        }
    }
}
