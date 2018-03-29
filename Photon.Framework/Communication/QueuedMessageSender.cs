using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Photon.Framework.Communication
{
    internal class QueuedMessageSender : IDisposable
    {
        private readonly Stream stream;
        private readonly object startStopLock;
        private ActionBlock<IRequestMessage> queue;
        private bool isStarted;


        public QueuedMessageSender(Stream stream)
        {
            this.stream = stream;

            startStopLock = new object();
        }

        public void Dispose()
        {
            stream?.Dispose();
        }

        public void Start()
        {
            lock (startStopLock) {
                if (isStarted) throw new Exception("Queue has already been started!");
                isStarted = true;
            }

            queue = new ActionBlock<IRequestMessage>(OnProcess);
        }

        public async Task StopAsync()
        {
            lock (startStopLock) {
                if (!isStarted) return;
                isStarted = false;
            }

            queue.Complete();
            await queue.Completion;
        }

        public void Send(IRequestMessage message)
        {
            queue.Post(message);
        }

        private async Task OnProcess(IRequestMessage message)
        {
            // TODO: Create a MemoryStream pool for reducing resources?
            using (var bufferStream = new MemoryStream()) {
                using (var writer = new BsonDataWriter(bufferStream)) {
                    var serializer = new JsonSerializer();
                    serializer.Serialize(writer, message);
                    //await writer.FlushAsync();
                }

                var size = bufferStream.Length;
                bufferStream.Seek(0, SeekOrigin.Begin);

                var sizeData = BitConverter.GetBytes(size);
                await stream.WriteAsync(sizeData, 0, sizeData.Length);
                await bufferStream.CopyToAsync(stream);
            }
        }
    }
}
