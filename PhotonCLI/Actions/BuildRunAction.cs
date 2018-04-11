using AnsiConsole;
using Newtonsoft.Json;
using Photon.CLI.Internal;
using Photon.CLI.Internal.Http;
using Photon.Framework;
using Photon.Framework.Extensions;
using Photon.Library.HttpMessages;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Photon.CLI.Actions
{
    internal class BuildRunAction
    {
        public string ServerName {get; set;}
        public string GitRefspec {get; set;}
        public string StartFile {get; set;}
        public HttpBuildResultResponse Result {get; set;}


        public async Task Run(CommandContext context)
        {
            var server = context.Servers.Get(ServerName);

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
                    await Task.Delay(400);
                    continue;
                }

                position = data.NewLength;

                ConsoleEx.Out.WriteLine(data.NewText, ConsoleColor.Gray);
            }

            Result = await GetResult(server, sessionId);
        }

        private async Task<HttpBuildStartResponse> StartSession(PhotonServerDefinition server)
        {
            var url = NetPath.Combine(server.Url, "build")
                +NetPath.QueryString(new {
                    refspec = GitRefspec,
                });

            try {
                var request = WebRequest.CreateHttp(url);
                request.Method = "POST";
                request.KeepAlive = true;

                using (var stream = File.Open(StartFile, FileMode.Open, FileAccess.Read)) {
                    request.ContentLength = stream.Length;

                    using (var requestStream = request.GetRequestStream()) {
                        await stream.CopyToAsync(requestStream);
                    }
                }

                using (var response = (HttpWebResponse) await request.GetResponseAsync()) {
                    //await response.PrintResponse();

                    if (response.StatusCode != HttpStatusCode.OK)
                        throw new ApplicationException($"Server Responded with [{(int) response.StatusCode}] {response.StatusDescription}");

                    using (var responseStream = response.GetResponseStream()) {
                        if (responseStream == null)
                            return null;

                        var serializer = new JsonSerializer();
                        return serializer.Deserialize<HttpBuildStartResponse>(responseStream);
                    }
                }
            }
            catch (WebException error) when (error.Response is HttpWebResponse response) {
                if (response.StatusCode == HttpStatusCode.NotFound)
                    throw new ApplicationException($"Photon-Server instance '{server.Name}' not found!");

                throw new ApplicationException($"Server Responded with [{(int) response.StatusCode}] {response.StatusDescription}");
            }
            catch (Exception error) {
                throw new ApplicationException("Failed to connect to Server instance!", error);
            }
        }

        private async Task<OutputData> UpdateOutput(PhotonServerDefinition server, string sessionId, int position)
        {
            var url = NetPath.Combine(server.Url, "session/output")
                +NetPath.QueryString(new {
                    session = sessionId,
                    start = position,
                });

            var request = WebRequest.CreateHttp(url);
            request.Method = "GET";
            request.KeepAlive = true;

            try {
                using (var response = (HttpWebResponse)await request.GetResponseAsync()) {
                    var result = new OutputData();

                    if (bool.TryParse(response.Headers.Get("X-Complete"), out var _complete))
                        result.IsComplete = _complete;

                    if (int.TryParse(response.Headers.Get("X-Text-Pos"), out var _textPos))
                        result.NewLength = _textPos;

                    if (response.StatusCode != HttpStatusCode.OK)
                        throw new ApplicationException($"Server Responded with [{(int)response.StatusCode}] {response.StatusDescription}");

                    using (var responseStream = response.GetResponseStream()) {
                        if (responseStream == null)
                            return result;

                        using (var reader = new StreamReader(responseStream)) {
                            result.NewText = reader.ReadToEnd();
                        }
                    }

                    result.IsModified = true;
                    return result;
                }
            }
            catch (WebException error) {
                if (error.Response is HttpWebResponse response) {
                    if (response.StatusCode == HttpStatusCode.NotModified) {
                        bool.TryParse(response.Headers.Get("X-Complete"), out var _complete);

                        return new OutputData {IsComplete = _complete};
                    }
                    
                    await response.PrintResponse();

                    throw new ApplicationException($"Server Responded with [{(int)response.StatusCode}] {response.StatusDescription}");
                }

                throw;
            }
        }

        private async Task<HttpBuildResultResponse> GetResult(PhotonServerDefinition server, string sessionId)
        {
            var url = NetPath.Combine(server.Url, "script/result")
                +NetPath.QueryString(new {
                    session = sessionId,
                });

            try {
                var request = WebRequest.CreateHttp(url);
                request.Method = "GET";
                request.KeepAlive = false;
                request.ContentLength = 0;

                using (var response = (HttpWebResponse) await request.GetResponseAsync()) {

                    if (response.StatusCode != HttpStatusCode.OK)
                        throw new ApplicationException($"Server Responded with [{(int) response.StatusCode}] {response.StatusDescription}");

                    using (var responseStream = response.GetResponseStream()) {
                        if (responseStream == null)
                            return null;

                        var serializer = new JsonSerializer();
                        var responseResult = serializer.Deserialize<HttpBuildResultResponse>(responseStream);
                        return responseResult;
                    }
                }
            }
            catch (WebException error) when (error.Response is HttpWebResponse response) {
                if (response.StatusCode == HttpStatusCode.NotFound)
                    throw new ApplicationException($"Photon-Server instance '{server.Name}' not found!");

                throw new ApplicationException($"Server Responded with [{(int) response.StatusCode}] {response.StatusDescription}");
            }
            catch (Exception error) {
                throw new ApplicationException("Failed to connect to Server instance!", error);
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
