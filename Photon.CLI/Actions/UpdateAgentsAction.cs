using Newtonsoft.Json;
using Photon.CLI.Internal;
using Photon.CLI.Internal.Http;
using Photon.Framework;
using Photon.Framework.Tools;
using Photon.Library;
using Photon.Library.HttpMessages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Photon.CLI.Actions
{
    internal class UpdateAgentsAction
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

            HttpAgentVersionListResponse agentVersionResponse;
            using (var webClient = new WebClient()) {
                var currentVersionUrl = NetPath.Combine(server.Url, "api/agent/versions");
                var json = (await webClient.DownloadStringTaskAsync(currentVersionUrl)).Trim();
                agentVersionResponse = JsonConvert.DeserializeObject<HttpAgentVersionListResponse>(json);
            }

            var updateAgents = new List<string>();
            foreach (var agentVersion in agentVersionResponse.VersionList) {
                if (!string.IsNullOrEmpty(agentVersion.Exception)) {
                    ConsoleEx.Out.Write("Failed to get version of agent ", ConsoleColor.DarkYellow)
                        .Write(agentVersion.AgentId, ConsoleColor.Yellow)
                        .WriteLine($"! {agentVersion.Exception}", ConsoleColor.DarkYellow);

                    continue;
                }

                if (!VersionTools.HasUpdates(agentVersion.AgentVersion, latestVersion)) {
                    ConsoleEx.Out.Write("Agent ", ConsoleColor.DarkBlue)
                        .Write(agentVersion.AgentId, ConsoleColor.Blue)
                        .WriteLine(" is up-to-date.", ConsoleColor.DarkBlue);

                    continue;
                }

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

            if (!Directory.Exists(updateDirectory))
                Directory.CreateDirectory(updateDirectory);

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
            HttpClientEx client = null;

            try {
                var url = NetPath.Combine(server.Url, "api/agent/update/start");

                client = HttpClientEx.Post(url);
                client.Query = new {
                    agents = agentIds,
                };

                client.BodyFunc = () => File.Open(updateFilename, FileMode.Open, FileAccess.Read);

                await client.Send();

                if (client.ResponseBase.StatusCode == HttpStatusCode.BadRequest) {
                    var text = await client.GetResponseTextAsync();
                    throw new ApplicationException($"Bad Update Request! {text}");
                }

                return client.ParseJsonResponse<HttpAgentUpdateStartResponse>();
            }
            catch (HttpStatusCodeException error) {
                if (error.HttpCode == HttpStatusCode.NotFound)
                    throw new ApplicationException($"Photon-Server instance '{server.Name}' not found!");

                throw;
            }
            finally {
                client?.Dispose();
            }
        }

        private async Task<OutputData> UpdateOutput(PhotonServerDefinition server, string sessionId, int position)
        {
            HttpClientEx client = null;

            try {
                var url = NetPath.Combine(server.Url, "api/session/output");

                client = HttpClientEx.Get(url, new {
                    session = sessionId,
                    start = position,
                });

                await client.Send();

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
            }
            catch (HttpStatusCodeException error) {
                if (error.HttpCode == HttpStatusCode.NotFound)
                    throw new ApplicationException($"Photon-Server instance '{server.Name}' not found!");

                throw;
            }
            finally {
                client?.Dispose();
            }
        }

        private async Task<HttpAgentUpdateResultResponse> GetResult(PhotonServerDefinition server, string sessionId)
        {
            HttpClientEx client = null;

            try {
                var url = NetPath.Combine(server.Url, "api/agent/update/result");

                client = HttpClientEx.Get(url, new {
                    session = sessionId,
                });

                await client.Send();

                return client.ParseJsonResponse<HttpAgentUpdateResultResponse>();
            }
            catch (HttpStatusCodeException error) {
                if (error.HttpCode == HttpStatusCode.NotFound)
                    throw new ApplicationException($"Photon-Server instance '{server.Name}' not found!");

                throw;
            }
            finally {
                client?.Dispose();
            }
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
