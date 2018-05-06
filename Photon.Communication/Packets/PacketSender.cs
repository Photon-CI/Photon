using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Photon.Communication.Messages;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Communication.Packets
{
    internal class PacketSender : IDisposable
    {
        private readonly BinaryWriter writer;
        private readonly ManualResetEventSlim waitEvent;
        private readonly JsonSerializer jsonSerializer;
        private CancellationTokenSource tokenSource;
        private Task task;

        private readonly ConcurrentQueue<IMessage> messageQueue;
        private readonly List<PacketSource> packetSourceList;

        public int BufferSize {get;}


        public PacketSender(BinaryWriter writer, int bufferSize)
        {
            this.writer = writer;
            this.BufferSize = bufferSize;

            if (bufferSize <= 0) throw new ArgumentOutOfRangeException(nameof(bufferSize), "Value must be greater than zero!");

            BufferSize = 4;
            packetSourceList = new List<PacketSource>();
            messageQueue = new ConcurrentQueue<IMessage>();
            jsonSerializer = new JsonSerializer();
            waitEvent = new ManualResetEventSlim();
        }

        public void Dispose()
        {
            //try {tokenSource?.Cancel();}
            //catch {}

            foreach (var packetSource in packetSourceList)
                packetSource.Dispose();

            packetSourceList.Clear();
            tokenSource?.Dispose();
            waitEvent?.Dispose();
        }

        public void Start()
        {
            tokenSource = new CancellationTokenSource();
            task = Task.Run(Process);
        }

        public async Task StopAsync()
        {
            tokenSource.Cancel();
            waitEvent.Set();
            await task;
        }

        public void Enqueue(IMessage message)
        {
            messageQueue.Enqueue(message);

            if (!waitEvent.IsSet)
                waitEvent.Set();
        }

        private async Task Process()
        {
            while (!tokenSource.IsCancellationRequested) {
                var hasAny = await OnProcess();

                if (!hasAny && messageQueue.IsEmpty) {
                    waitEvent.Reset();
                    waitEvent.Wait();
                }
            }
        }

        private async Task<bool> OnProcess()
        {
            var emptyCount = BufferSize - packetSourceList.Count;

            if (emptyCount > 0) {
                for (var i = 0; i < emptyCount; i++) {
                    if (messageQueue.IsEmpty) break;

                    if (!messageQueue.TryDequeue(out var message))
                        continue;

                    var packetSource = await CreatePacketSource(message);
                    packetSourceList.Add(packetSource);
                }
            }

            var count = packetSourceList.Count;
            for (var i = count - 1; i >= 0; i--) {
                var packetSource = packetSourceList[i];

                var packet = await packetSource.TryTakePacket();

                if (packet != null)
                    await packet.WriteToAsync(writer);

                if (packetSource.IsComplete) {
                    packetSourceList.RemoveAt(i);
                    packetSource.Dispose();
                }
            }

            return count > 0;
        }

        private async Task<PacketSource> CreatePacketSource(IMessage message)
        {
            var messageId = message.MessageId;
            var messageType = message.GetType().AssemblyQualifiedName;
            var messageData = new MemoryStream();

            Stream streamData = null;
            if (message is IStreamMessage streamMessage) {
                streamData = streamMessage.StreamFunc();
            }
            else if (message is IFileMessage fileMessage) {
                streamData = File.Open(fileMessage.Filename, FileMode.Open, FileAccess.Read);
            }

            // TODO: BsonDataWriter should be disposed!
            //   but it will close the stream.
            var bsonWriter = new BsonDataWriter(messageData);

            jsonSerializer.Serialize(bsonWriter, message);
            await bsonWriter.FlushAsync();

            messageData.Seek(0, SeekOrigin.Begin);

            return new PacketSource(messageId, messageType) {
                MessageData = messageData,
                StreamData = streamData,
            };
        }
    }
}
