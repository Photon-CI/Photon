using Newtonsoft.Json;
using Photon.CLI.Internal;
using Photon.Framework;
using Photon.Library.Http.Messages;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Photon.CLI.Actions
{
    internal class DeployRunAction : ActionBase
    {
        private const int PollIntervalMs = 400;

        public string ServerName {get; set;}
        public string ProjectId {get; set;}
        public string ProjectPackageId {get; set;}
        public string ProjectPackageVersion {get; set;}
        public string Environment {get; set;}
        public HttpDeployResultResponse Result {get; set;}


        public async Task Run(CommandContext context)
        {
            var server = context.Servers.Get(ServerName);

            HttpSessionStartResponse startResult = null;
            await AuthRetryAsync(async () => {
                startResult = await StartSession(server);
            });

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
        }

        private async Task<HttpSessionStartResponse> StartSession(PhotonServerDefinition server)
        {
            return await WebClientEx(server,
                client => {
                    client.Url = NetPath.Combine(server.Url, "api/deploy/start");
                    client.Method = "POST";
                    client.Query = new {
                        project = ProjectId,
                        package = ProjectPackageId,
                        version = ProjectPackageVersion,
                        env = Environment,
                    };
                },
                client => client.ParseJsonResponse<HttpSessionStartResponse>());
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

        private async Task<HttpDeployResultResponse> GetResult(PhotonServerDefinition server, string sessionId)
        {
            return await WebClient(server, async client => {
                client.QueryString["session"] = sessionId;

                var json = await client.DownloadStringTaskAsync("api/deploy/result");
                return JsonConvert.DeserializeObject<HttpDeployResultResponse>(json);
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
