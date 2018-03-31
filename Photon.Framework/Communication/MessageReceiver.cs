using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Framework.Communication
{
    internal class MessageReceiver : IDisposable
    {
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        private readonly JsonSerializer serializer;
        private Stream stream;
        private BinaryReader reader;
        private Task task;
        private CancellationTokenSource tokenSource;


        public MessageReceiver()
        {
            serializer = new JsonSerializer();
        }

        public void Dispose()
        {
            reader?.Dispose();
            stream?.Dispose();
        }

        public void Start(Stream stream)
        {
            this.stream = stream;

            reader = new BinaryReader(stream, Encoding.UTF8, true);

            tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;

            task = Task.Run(async () => {
                while (!token.IsCancellationRequested)
                    await ReadMessage();
            });
        }

        public async Task StopAsync()
        {
            tokenSource.Cancel();
            await task;
        }

        protected virtual void OnMessageReceived(IMessage message)
        {
            MessageReceived?.Invoke(this, new MessageReceivedEventArgs(message));
        }

        private async Task ReadMessage()
        {
            var messageTypeName = reader.ReadString();
            var messageLength = reader.ReadInt64();

            var messageType = Type.GetType(messageTypeName);
            if (messageType == null) throw new Exception($"Unknown message type '{messageTypeName}'!");

            using (var bufferStream = new MemoryStream()) {
                await CopyLengthAsync(stream, bufferStream, messageLength);
                bufferStream.Seek(0, SeekOrigin.Begin);

                object messageData;
                using (var bsonReader = new BsonDataReader(bufferStream)) {
                    messageData = serializer.Deserialize(bsonReader, messageType);
                }

                if (messageData == null) throw new Exception("Failed to parse message!");

                OnMessageReceived(messageData as IMessage);
            }
        }

        private async Task CopyLengthAsync(Stream sourceStream, Stream destStream, long length)
        {
            const int buffer_size = 4096;
            var buffer = new byte[buffer_size];

            var position = 0;
            while (position < length) {
                var count = (int)Math.Min(length - position, buffer_size);
                var size = await stream.ReadAsync(buffer, 0, count);
                destStream.Write(buffer, 0, size);
                position += size;
            }
        }

        private async Task<byte[]> ReadBytesAsync(int size)
        {
            var buffer = new byte[size];

            var position = 0;
            while (position < size) {
                position += await stream.ReadAsync(buffer, position, size - position);
            }

            return buffer;
        }
    }
}
