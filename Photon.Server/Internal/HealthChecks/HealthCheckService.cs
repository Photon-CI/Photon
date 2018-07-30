using log4net;
using Photon.Communication;
using Photon.Framework.Extensions;
using Photon.Framework.Server;
using Photon.Library.TcpMessages;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Photon.Server.Internal.HealthChecks
{
    internal class HealthCheckService : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(HealthCheckService));

        private readonly ConcurrentDictionary<string, AgentStatusItem> items;
        private CancellationTokenSource tokenSource;
        private Task runTask;

        public TimeSpan Interval {get; set;}
        public int Parallelism {get; set;}


        public HealthCheckService()
        {
            Interval = TimeSpan.FromMinutes(15);
            Parallelism = 8;

            items = new ConcurrentDictionary<string, AgentStatusItem>(StringComparer.Ordinal);
        }

        public void Dispose()
        {
            tokenSource?.Dispose();
        }

        public void Start()
        {
            tokenSource = new CancellationTokenSource();
            runTask = Run(tokenSource.Token);
        }

        public void Stop()
        {
            tokenSource.Cancel();
        }

        public AgentStatusItem GetStatus(string agentId)
        {
            return items.TryGetValue(agentId, out var item) ? item : null;
        }

        private async Task Run(CancellationToken token)
        {
            while (true) {
                token.ThrowIfCancellationRequested();

                var agents = PhotonServer.Instance.Agents.All.ToArray();

                foreach (var key in items.Keys) {
                    if (!agents.Any(a => string.Equals(a.Id, key, StringComparison.Ordinal)))
                        items.TryRemove(key, out _);
                }

                var queueOptions = new ExecutionDataflowBlockOptions {
                    MaxDegreeOfParallelism = Parallelism,
                    CancellationToken = token,
                };

                var lastUpdate = DateTime.Now;
                var queue = new ActionBlock<ServerAgent>(async agent => await RunAgent(agent, token), queueOptions);

                foreach (var agent in agents)
                    queue.Post(agent);

                queue.Complete();

                try {
                    await queue.Completion;
                }
                catch (Exception error) {
                    Log.Warn("An error occurred while performing health checks!", error);
                }

                var duration = DateTime.Now - lastUpdate;
                var waitTime = Interval - duration;

                if (waitTime.TotalMilliseconds > 10)
                    await Task.Delay(waitTime, token);
            }
        }

        private async Task RunAgent(ServerAgent agent, CancellationToken token)
        {
            var result = await GetAgentStatus(agent, token);

            var item = new AgentStatusItem {
                AgentId = agent.Id,
                AgentName = agent.Name,
                Status = result.Status,
                Warnings = result.Warnings?.ToArray(),
                Errors = result.Errors?.ToArray(),
            };

            items.AddOrUpdate(agent.Id, item, (x, y) => item);
        }

        private async Task<HealthCheckResult> GetAgentStatus(ServerAgent agent, CancellationToken token)
        {
            using (var messageClient = new MessageClient(PhotonServer.Instance.MessageRegistry)) {
                try {
                    await messageClient.ConnectAsync(agent.TcpHost, agent.TcpPort, token);

                    await ClientHandshake.Verify(messageClient, token);
                }
                catch (Exception error) {
                    return new HealthCheckResult {
                        Status = AgentStatus.Disconnected,
                        Errors = {
                            error.UnfoldMessages(),
                        },
                    };
                }

                var request = new HealthCheckRequest();

                try {
                    var response = await messageClient.Send(request)
                        .GetResponseAsync<HealthCheckResponse>(token);

                    var status = AgentStatus.Ok;

                    if (response.Errors?.Any() ?? false)
                        status = AgentStatus.Error;
                    else if (response.Warnings?.Any() ?? false)
                        status = AgentStatus.Warning;

                    return new HealthCheckResult {
                        Status = status,
                        Warnings = response.Warnings,
                        Errors = response.Errors,
                    };
                }
                catch (Exception error) {
                    return new HealthCheckResult {
                        Status = AgentStatus.Error,
                        Errors = {
                            error.UnfoldMessages(),
                        },
                    };
                }
                finally {
                    try {
                        messageClient.Disconnect();
                    }
                    catch {}
                }
            }
        }
    }

    internal class HealthCheckResult
    {
        public AgentStatus Status {get; set;}
        public List<string> Warnings {get; set;}
        public List<string> Errors {get; set;}


        public HealthCheckResult()
        {
            Warnings = new List<string>();
            Errors = new List<string>();
        }
    }
}
