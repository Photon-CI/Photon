using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Photon.Framework.Communication;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Photon.Framework.Communication
{
    internal class QueuedMessageSender : IDisposable
    {
        private readonly object startStopLock;
        private readonly JsonSerializer serializer;
        //private readonly BsonDataWriter bsonWriter;
        private ActionBlock<IMessage> queue;
        private Stream stream;
        private BinaryWriter writer;
        private bool isStarted;


        public QueuedMessageSender()
        {
            serializer = new JsonSerializer();
            startStopLock = new object();
        }

        public void Dispose()
        {
            writer?.Dispose();
            stream?.Dispose();
        }

        public void Start(Stream stream)
        {
            lock (startStopLock) {
                if (isStarted) throw new Exception("Queue has already been started!");
                isStarted = true;
            }

            this.stream = stream;

            writer = new BinaryWriter(stream, Encoding.UTF8, true);
            queue = new ActionBlock<IMessage>(OnProcess);
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

        public void Send(IMessage message)
        {
            queue.Post(message);
        }

        private async Task OnProcess(IMessage message)
        {
            var messageType = message.GetType().AssemblyQualifiedName;

            // TODO: Create a MemoryStream pool for reducing resources?
            using (var bufferStream = new MemoryStream()) {
                using (var writer = new BsonDataWriter(bufferStream)) {
                    serializer.Serialize(writer, message);
                    //await writer.FlushAsync();
                }

                bufferStream.Seek(0, SeekOrigin.Begin);

                writer.Write(messageType);
                writer.Write(bufferStream.Length);
                writer.Flush();

                await bufferStream.CopyToAsync(stream);
                //await stream.FlushAsync();
            }
        }
    }
}
