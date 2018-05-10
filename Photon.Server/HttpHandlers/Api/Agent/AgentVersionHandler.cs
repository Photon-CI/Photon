using log4net;
using Photon.Communication;
using Photon.Framework.Extensions;
using Photon.Framework.Server;
using Photon.Library.Extensions;
using Photon.Library.HttpMessages;
using Photon.Library.TcpMessages;
using Photon.Server.Internal;
using PiServerLite.Http.Handlers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Photon.Server.HttpHandlers.Api.Agent
{
    internal class AgentVersionInfo
    {
        public string Version {get; set;}
        public string Exception {get; set;}
    }

    [HttpHandler("api/agent/versions")]
    internal class AgentVersionHandler : HttpHandlerAsync
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(AgentVersionHandler));


        public override async Task<HttpHandlerResult> GetAsync(CancellationToken token)
        {
            var _names = GetQuery("names");

            try {
                // Get Agent Names
                string[] agentNames = null;

                if (!string.IsNullOrEmpty(_names))
                    agentNames = ParseNames(_names).ToArray();

                var agents = PhotonServer.Instance.Agents.All
                    .Where(x => IncludesAgent(agentNames, x)).ToArray();

                if (!agents.Any()) throw new ApplicationException("No agents were found!");

                // Get Agent Versions
                var versionMap = new ConcurrentDictionary<string, AgentVersionInfo>();

                var blockOptions = new ExecutionDataflowBlockOptions {
                    MaxDegreeOfParallelism = Configuration.Parallelism,
                    CancellationToken = token,
                };

                var block = new ActionBlock<ServerAgent>(async agent => {
                    var result = new AgentVersionInfo();

                    try {
                        result.Version = await GetAgentVersion(agent, token);
                    }
                    catch (Exception error) {
                        result.Exception = error.UnfoldMessages();
                    }

                    versionMap[agent.Id] = result;
                }, blockOptions);

                foreach (var agent in agents)
                    block.Post(agent);

                block.Complete();
                await block.Completion;

                // Send Response
                var response = new HttpAgentVersionListResponse {
                    VersionList = versionMap.Select(x => new AgentVersionResponse {
                        AgentId = x.Key,
                        AgentVersion = x.Value.Version,
                        Exception = x.Value.Exception,
                    }).ToArray(),
                };

                return Response.Json(response);
            }
            catch (Exception error) {
                Log.Error("Failed to run Update-Task!", error);
                return Response.Exception(error);
            }
        }

        private static async Task<string> GetAgentVersion(ServerAgent agent, CancellationToken token)
        {
            MessageClient messageClient = null;

            try {
                messageClient = new MessageClient(PhotonServer.Instance.MessageRegistry);
                await messageClient.ConnectAsync(agent.TcpHost, agent.TcpPort, token);

                var handshakeRequest = new HandshakeRequest {
                    Key = Guid.NewGuid().ToString(),
                    ServerVersion = Configuration.Version,
                };

                var timeout = TimeSpan.FromSeconds(30);
                var handshakeResponse = await messageClient.Handshake<HandshakeResponse>(handshakeRequest, timeout, token);

                if (!string.Equals(handshakeRequest.Key, handshakeResponse.Key, StringComparison.Ordinal))
                    throw new ApplicationException("Handshake Failed! An invalid key was returned.");

                if (!handshakeResponse.PasswordMatch)
                    throw new ApplicationException("Handshake Failed! Unauthorized.");

                var agentVersionRequest = new AgentGetVersionRequest();

                var agentVersionResponse = await messageClient.Send(agentVersionRequest)
                    .GetResponseAsync<AgentGetVersionResponse>(token);

                return agentVersionResponse.Version;
            }
            catch (Exception error) {
                throw new ApplicationException($"Failed to retrieve version of agent '{agent.Name}'!", error);
            }
            finally {
                messageClient?.Dispose();
            }
        }

        private static IEnumerable<string> ParseNames(string names)
        {
            return names.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim());
        }

        private static bool IncludesAgent(string[] names, ServerAgent agent)
        {
            if (!(names?.Any() ?? false)) return true;

            foreach (var name in names) {
                var escapedName = Regex.Escape(name)
                    .Replace("\\?", ".")
                    .Replace("\\*", ".*");

                var namePattern = $"^{escapedName}$";
                if (Regex.IsMatch(agent.Name, namePattern, RegexOptions.IgnoreCase))
                    return true;
            }

            return false;
        }
    }
}
