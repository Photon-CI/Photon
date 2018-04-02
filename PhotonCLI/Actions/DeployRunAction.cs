using Photon.CLI.Internal;
using Photon.Framework;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using ConsoleEx = AnsiConsole.AnsiConsole;

namespace Photon.CLI.Actions
{
    internal class DeployRunAction
    {
        public string ServerName {get; set;}
        public string ProjectName {get; set;}
        public string ProjectVersion {get; set;}
        public string ScriptName {get; set;}


        public async Task Run(CommandContext context)
        {
            ServerDefinition server;
            if (ServerName != null) {
                server = context.Servers.Get(ServerName);

                if (server == null) throw new ApplicationException($"Server '{ServerName}' could not be found!");
            }
            else {
                server = context.Servers.GetPrimary();

                if (server == null) throw new ApplicationException("No primary Server could not be found!");
            }

            var url = NetPath.Combine(server.Url, "deploy")
                +NetPath.QueryString(new {
                    project = ProjectName,
                    version = ProjectVersion,
                    script = ScriptName,
                });

            var request = WebRequest.CreateHttp(url);
            request.Method = "POST";
            request.KeepAlive = false;
            request.ContentLength = 0;

            try {
                using (var response = (HttpWebResponse)await request.GetResponseAsync()) {
                    if (response.StatusCode != HttpStatusCode.OK)
                        throw new ApplicationException($"Server Responded with [{(int)response.StatusCode}] {response.StatusDescription}");

                    using (var responseStream = response.GetResponseStream()) {
                        if (responseStream != null) {
                            using (var responseReader = new StreamReader(responseStream)) {
                                while (!responseReader.EndOfStream) {
                                    var line = await responseReader.ReadLineAsync();
                                    ConsoleEx.Out.WriteLine(line, ConsoleColor.Gray);
                                }
                            }
                        }
                    }
                }
            }
            catch (WebException error) {
                if (error.Response is HttpWebResponse response)
                    throw new ApplicationException($"Server Responded with [{(int)response.StatusCode}] {response.StatusDescription}");

                throw;
            }
        }
    }
}
