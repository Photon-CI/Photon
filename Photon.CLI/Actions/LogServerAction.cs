using Photon.CLI.Internal;
using Photon.Framework;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Photon.CLI.Actions
{
    internal class LogServerAction
    {
        public string ServerName {get; set;}


        public async Task Run(CommandContext context)
        {
            var server = context.Servers.Get(ServerName);

            var url = NetPath.Combine(server.Url, "api/log");
            var request = WebRequest.CreateHttp(url);
            request.Method = "GET";
            request.KeepAlive = false;

            using (var response = (HttpWebResponse) await request.GetResponseAsync())
            using (var responseStream = response.GetResponseStream()) {
                if (responseStream == null) return;

                using (var reader = new StreamReader(responseStream)) {
                    while (!reader.EndOfStream) {
                        var line = await reader.ReadLineAsync();
                        Console.ResetColor();
                        Console.WriteLine(line);
                    }
                }
            }
        }
    }
}
