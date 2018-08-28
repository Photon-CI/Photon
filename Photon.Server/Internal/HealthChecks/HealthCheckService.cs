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
using System.Timers;
using Timer = System.Timers.Timer;

namespace Photon.Server.Internal.HealthChecks
{
    internal class HealthCheckService : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(HealthCheckService));

        private readonly ConcurrentDictionary<string, AgentStatusItem> items;
        private readonly CancellationTokenSource tokenSource;
        private readonly Timer timer;
        private ActionBlock<ServerAgent> queue;

        public int Parallelism {get; set;}
        public TimeSpan AgentTimeout {get; set;}
        public DateTime LastUpdate {get; private set;}

        public TimeSpan Interval {
            get => TimeSpan.FromMilliseconds(timer.Interval);
            set => timer.Interval = value.TotalMilliseconds;
        }


        public HealthCheckService()
        {
            items = new ConcurrentDictionary<string, AgentStatusItem>(StringComparer.Ordinal);
            tokenSource = new CancellationTokenSource();
            timer = new Timer();
            timer.Elapsed += Timer_OnElapsed;

            Interval = TimeSpan.FromMinutes(6);
            AgentTimeout = TimeSpan.FromMinutes(2);
            Parallelism = 8;
        }

        public void Dispose()
        {
            tokenSource?.Dispose();
            timer?.Dispose();
            tokenSource?.Dispose();
        }

        public void Start()
        {
            var queueOptions = new ExecutionDataflowBlockOptions {
                MaxDegreeOfParallelism = Parallelism,
                CancellationToken = tokenSource.Token,
            };

            queue = new ActionBlock<ServerAgent>(RunAgent, queueOptions);

            timer.Start();

            RunAllAgents();
        }

        public void Stop(bool abort = false)
        {
            timer.Stop();
            queue.Complete();

            if (abort) tokenSource.Cancel();

            try {
                Task.WaitAll(queue.Completion);
            }
            catch (Exception error) {
                Log.Warn("An error occurred while performing health checks!", error);
            }
        }

        public AgentStatusItem GetStatus(string agentId)
        {
            return items.TryGetValue(agentId, out var item) ? item : null;
        }

        private void RunAllAgents()
        {
            LastUpdate = DateTime.UtcNow;

            var agents = PhotonServer.Instance.Agents.All.ToArray();

            foreach (var key in items.Keys) {
                if (!agents.Any(a => string.Equals(a.Id, key, StringComparison.Ordinal)))
                    items.TryRemove(key, out _);
            }

            foreach (var agent in agents)
                queue.Post(agent);
        }

        private async Task RunAgent(ServerAgent agent)
        {
            var item = new AgentStatusItem {
                AgentId = agent.Id,
                AgentName = agent.Name,
            };

            try {
                HealthCheckResult result;
                using (var timeoutTokenSource = new CancellationTokenSource(AgentTimeout))
                using (var mergedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(tokenSource.Token, timeoutTokenSource.Token)) {
                    result = await GetAgentStatus(agent, mergedTokenSource.Token);
                }

                item.AgentVersion = result.Version;
                item.Status = result.Status;
                item.Warnings = result.Warnings?.ToArray();
                item.Errors = result.Errors?.ToArray();
            }
            catch (TaskCanceledException) {
                item.Status = AgentStatus.Error;
                item.Warnings = new[] {
                    "A timeout occurred while performing the health check.",
                };
            }
            catch (Exception error) {
                item.Status = AgentStatus.Error;
                item.Errors = new [] {
                    $"Health-check failed! {error.UnfoldMessages()}",
                };
            }

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
                        Version = response.AgentVersion,
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

        private void Timer_OnElapsed(object sender, ElapsedEventArgs e)
        {
            RunAllAgents();
        }
    }

    internal class HealthCheckResult
    {
        public AgentStatus Status {get; set;}
        public string Version {get; set;}
        public List<string> Warnings {get; set;}
        public List<string> Errors {get; set;}


        public HealthCheckResult()
        {
            Warnings = new List<string>();
            Errors = new List<string>();
        }
    }
}
