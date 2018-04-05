using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Photon.CLI.Internal;
using Photon.CLI.Internal.Http;
using Photon.Framework;

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
                    await response.PrintResponse();

                    if (response.StatusCode != HttpStatusCode.OK)
                        throw new ApplicationException($"Server Responded with [{(int)response.StatusCode}] {response.StatusDescription}");
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
    }
}
