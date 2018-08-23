using Newtonsoft.Json;
using Photon.CLI.Internal;
using Photon.Framework;
using Photon.Framework.Tools;
using Photon.Library;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Photon.Library.Http.Messages;

namespace Photon.CLI.Actions
{
    internal class UpdateAgentsAction : ActionBase
    {
        private const int PollIntervalMs = 400;

        public string ServerName {get; set;}
        public string AgentNames {get; set;}
        public HttpAgentUpdateResultResponse Result {get; set;}


        public async Task Run(CommandContext context)
        {
            var server = context.Servers.Get(ServerName);

            ConsoleEx.Out.WriteLine("Retrieving latest agent version...", ConsoleColor.DarkCyan);

            var agentIndex = await DownloadTools.GetLatestAgentIndex();
            var latestVersion = agentIndex.Version;

            ConsoleEx.Out
                .Write("Found Latest Version ", ConsoleColor.DarkCyan)
                .WriteLine(latestVersion, ConsoleColor.Cyan)
                .WriteLine("Checking agent versions...", ConsoleColor.DarkCyan);

            HttpAgentVersionListResponse agentVersionResponse = null;

            await HttpAuthAsync(async () => {
                agentVersionResponse = await WebClient(server, async client => {
                    var json = (await client.DownloadStringTaskAsync("api/agent/versions")).Trim();
                    return JsonConvert.DeserializeObject<HttpAgentVersionListResponse>(json);
                });
            });

            var updateAgents = new List<string>();
            foreach (var agentVersion in agentVersionResponse.VersionList) {
                if (!string.IsNullOrEmpty(agentVersion.Exception)) {
                    ConsoleEx.Out.Write("Failed to get version of agent ", ConsoleColor.DarkYellow)
                        .Write(agentVersion.AgentName, ConsoleColor.Yellow)
                        .WriteLine($"! {agentVersion.Exception}", ConsoleColor.DarkYellow);

                    continue;
                }

                if (!VersionTools.HasUpdates(agentVersion.AgentVersion, latestVersion)) {
                    ConsoleEx.Out.Write("Agent ", ConsoleColor.DarkBlue)
                        .Write(agentVersion.AgentName, ConsoleColor.Blue)
                        .WriteLine(" is up-to-date.", ConsoleColor.DarkBlue);

                    continue;
                }

                ConsoleEx.Out.Write("Updating ", ConsoleColor.DarkCyan)
                    .Write(agentVersion.AgentName, ConsoleColor.Cyan)
                    .Write(" from version ", ConsoleColor.DarkCyan)
                    .Write(agentVersion.AgentVersion, ConsoleColor.Cyan)
                    .WriteLine(".", ConsoleColor.DarkCyan);

                updateAgents.Add(agentVersion.AgentId);
            }

            if (!updateAgents.Any()) {
                ConsoleEx.Out.WriteLine("All agents are up-to-date.", ConsoleColor.DarkGreen);
                return;
            }

            // Download update msi

            ConsoleEx.Out.Write("Downloading Agent update ", ConsoleColor.DarkCyan)
                .Write(agentIndex.Version, ConsoleColor.Cyan)
                .WriteLine("...", ConsoleColor.DarkCyan);

            var url = NetPath.Combine(Configuration.DownloadUrl, "agent", agentIndex.Version, agentIndex.MsiFilename);

            var updateDirectory = Path.Combine(Configuration.Directory, "Updates");
            var updateFilename = Path.Combine(updateDirectory, "Photon.Agent.msi");

            PathEx.CreatePath(updateDirectory);

            using (var client = new WebClient()) {
                await client.DownloadFileTaskAsync(url, updateFilename);
            }

            ConsoleEx.Out.WriteLine("Download Complete.", ConsoleColor.DarkBlue);

            // Perform updates

            var agentIdList = updateAgents.ToArray();

            var startResult = await StartSession(server, agentIdList, updateFilename);
            var sessionId = startResult?.SessionId;

            if (string.IsNullOrEmpty(sessionId))
                throw new ApplicationException($"An invalid session-id was returned! [{sessionId}]");

            var position = 0;
            while (true) {
                var data = await UpdateOutput(server, sessionId, position);

                if (data == null) throw new ApplicationException("An empty session-output response was returned!");

                if (data.IsComplete) break;

                if (!data.IsModified) {
                    await Task.Delay(PollIntervalMs);
                    continue;
                }

                position = data.NewLength;

                ConsoleEx.Out.WriteLine(data.NewText, ConsoleColor.Gray);
            }

            Result = await GetResult(server, sessionId);

            ConsoleEx.Out.WriteLine("Update completed successfully.", ConsoleColor.DarkGreen);
        }

        private async Task<HttpAgentUpdateStartResponse> StartSession(PhotonServerDefinition server, string[] agentIds, string updateFilename)
        {
            return await WebClientEx(server,
                client => {
                    client.Method = "POST";
                    client.Url = NetPath.Combine(server.Url, "api/agent/update/start");
                    client.Query = new {
                        agents = string.Join(";", agentIds),
                    };

                    client.BodyFunc = () => File.Open(updateFilename, FileMode.Open, FileAccess.Read);
                },
                client => client.ParseJsonResponse<HttpAgentUpdateStartResponse>());
        }

        private async Task<OutputData> UpdateOutput(PhotonServerDefinition server, string sessionId, int position)
        {
            return await WebClientEx(server,
                client => {
                    client.Url = NetPath.Combine(server.Url, "api/session/output");
                    client.Query = new {
                        session = sessionId,
                        start = position,
                    };
                },
                client => {
                    bool _complete;
                    if (client.ResponseBase.StatusCode == HttpStatusCode.NotModified) {
                        bool.TryParse(client.ResponseBase.Headers.Get("X-Complete"), out _complete);

                        return new OutputData {IsComplete = _complete};
                    }

                    var result = new OutputData();

                    if (bool.TryParse(client.ResponseBase.Headers.Get("X-Complete"), out _complete))
                        result.IsComplete = _complete;

                    if (int.TryParse(client.ResponseBase.Headers.Get("X-Text-Pos"), out var _textPos))
                        result.NewLength = _textPos;

                    using (var responseStream = client.ResponseBase.GetResponseStream()) {
                        if (responseStream == null)
                            return result;

                        using (var reader = new StreamReader(responseStream)) {
                            result.NewText = reader.ReadToEnd();
                        }
                    }

                    result.IsModified = true;
                    return result;
                });
        }

        private async Task<HttpAgentUpdateResultResponse> GetResult(PhotonServerDefinition server, string sessionId)
        {
            return await WebClient(server, async client => {
                client.QueryString["session"] = sessionId;

                var json = await client.DownloadStringTaskAsync("api/agent/update/result");
                return JsonConvert.DeserializeObject<HttpAgentUpdateResultResponse>(json);
            });
        }

        private class OutputData
        {
            public bool IsModified {get; set;}
            public bool IsComplete {get; set;}
            public string NewText {get; set;}
            public int NewLength {get; set;}
        }
    }
}
