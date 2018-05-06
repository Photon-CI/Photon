using Photon.CLI.Internal;
using Photon.CLI.Internal.Http;
using Photon.Framework;
using Photon.Library.HttpMessages;
using System;
using System.IO;
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

            //var agentIndex = await DownloadTools.GetLatestAgentIndex();

            ConsoleEx.Out.WriteLine("Checking agent version...", ConsoleColor.DarkCyan);

            string currentVersion;
            using (var webClient = new WebClient()) {
                var currentVersionUrl = NetPath.Combine(server.Url, "api/version");
                currentVersion = (await webClient.DownloadStringTaskAsync(currentVersionUrl)).Trim();
            }


            var startResult = await StartSession(server);
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

        private async Task<HttpAgentUpdateStartResponse> StartSession(PhotonServerDefinition server)
        {
            HttpClientEx client = null;

            try {
                var url = NetPath.Combine(server.Url, "api/agent/update/start");

                client = HttpClientEx.Post(url);
                client.Query = new {
                    names = AgentNames,
                };

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
