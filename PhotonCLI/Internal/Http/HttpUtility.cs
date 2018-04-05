using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using ConsoleEx = AnsiConsole.AnsiConsole;

namespace Photon.CLI.Internal.Http
{
    internal static class HttpUtility
    {
        public static async Task PrintResponse(this WebResponse response)
        {
            using (var responseStream = response.GetResponseStream()) {
                if (responseStream == null) return;

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
