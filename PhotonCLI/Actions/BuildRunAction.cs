using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Photon.CLI.Internal;
using Photon.CLI.Internal.Http;
using Photon.Framework;
using Photon.Framework.Extensions;
using Photon.Library.Messages;
using ConsoleEx = AnsiConsole.AnsiConsole;

namespace Photon.CLI.Actions
{
    internal class BuildRunAction
    {
        public string ServerName {get; set;}
        public string ProjectName {get; set;}
        public string GitRefspec {get; set;}
        public string AssemblyFile {get; set;}
        public string ScriptName {get; set;}
        public string StartFile {get; set;}


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

                ConsoleEx.Out.WriteLine(data.NewText);
            }
        }

        private async Task<BuildSessionBeginResponse> StartSession(PhotonServerDefinition server)
        {
            var url = NetPath.Combine(server.Url, "build")
                +NetPath.QueryString(new {
                    project = ProjectName,
                    refspec = GitRefspec,
                    assembly = AssemblyFile,
                    script = ScriptName,
                });

            var request = WebRequest.CreateHttp(url);
            request.Method = "POST";
            request.KeepAlive = false;

            if (!string.IsNullOrEmpty(StartFile)) {
                using (var stream = File.Open(StartFile, FileMode.Open, FileAccess.Read)) {
                    request.ContentLength = stream.Length;

                    using (var requestStream = request.GetRequestStream()) {
                        await stream.CopyToAsync(requestStream);
                    }
                }
            }
            else {
                request.ContentLength = 0;
            }

            try {
                using (var response = (HttpWebResponse)await request.GetResponseAsync()) {
                    //await response.PrintResponse();

                    if (response.StatusCode != HttpStatusCode.OK)
                        throw new ApplicationException($"Server Responded with [{(int)response.StatusCode}] {response.StatusDescription}");

                    using (var responseStream = response.GetResponseStream()) {
                        if (responseStream == null)
                            return null;

                        var serializer = new JsonSerializer();
                        return serializer.Deserialize<BuildSessionBeginResponse>(responseStream);
                    }
                }
            }
            catch (WebException error) {
                if (error.Response is HttpWebResponse response) {
                    await response.PrintResponse();

                    throw new ApplicationException($"Server Responded with [{(int)response.StatusCode}] {response.StatusDescription}");
                }

                throw;
            }
        }

        class OutputData
        {
            public bool IsModified {get; set;}
            public bool IsComplete {get; set;}
            public string NewText {get; set;}
            public int NewLength {get; set;}
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
            request.KeepAlive = false;

            try {
                using (var response = (HttpWebResponse)await request.GetResponseAsync()) {
                    var result = new OutputData();

                    if (bool.TryParse(response.Headers.Get("X-Complete"), out var _complete))
                        result.IsComplete = _complete;

                    if (int.TryParse(response.Headers.Get("X-Text-Pos"), out var _textPos))
                        result.NewLength = _textPos;

                    //if (response.StatusCode == HttpStatusCode.NotModified)
                    //    return result;

                    //await response.PrintResponse();

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
    }
}
