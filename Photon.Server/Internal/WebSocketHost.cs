using Newtonsoft.Json;
using System;
using System.IO;

namespace Photon.Server.Internal
{
    internal class WebSocketHost : IDisposable
    {
        private readonly Stream stream;
        private readonly StreamWriter writer;


        public WebSocketHost(Stream stream)
        {
            this.stream = stream;

            writer = new StreamWriter(stream) {
                NewLine = "\n",
                AutoFlush = false,
            };
        }

        public void Dispose()
        {
            writer?.Flush();
            writer?.Dispose();
            stream?.Dispose();
        }

        public void Send(string eventName, object eventData = null)
        {
            writer.WriteLine($"event: {eventName}");

            if (eventData != null) {
                var json = JsonConvert.SerializeObject(eventData);
                var lines = json.Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries);

                foreach (var line in lines)
                    writer.WriteLine($"data: {line}");
            }

            writer.WriteLine();
            writer.Flush();
        }
    }
}
