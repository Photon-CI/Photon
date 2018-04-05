using Photon.CLI.Internal;
using Photon.CLI.Internal.Http;
using Photon.Framework;
using System;
using System.Net;
using System.Threading.Tasks;

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
            var server = context.Servers.Get(ServerName);

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
