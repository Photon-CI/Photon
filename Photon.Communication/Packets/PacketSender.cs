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
        public event ThreadExceptionEventHandler ThreadError;

        private readonly BinaryWriter writer;
        private readonly ManualResetEventSlim waitEvent;
        private readonly JsonSerializer jsonSerializer;
        private volatile bool enabled;
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
            try {
                tokenSource?.Cancel();
                waitEvent.Set();
            }
            catch { }

            foreach (var packetSource in packetSourceList)
                packetSource.Dispose();

            packetSourceList.Clear();
            tokenSource?.Dispose();
            waitEvent?.Dispose();
        }

        public void Start()
        {
            enabled = true;
            tokenSource = new CancellationTokenSource();
            task = Task.Run(Process);
        }

        public async Task StopAsync()
        {
            enabled = false;
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
                var token = tokenSource.Token;

                var hasAny = await OnProcess(token);

                if (!hasAny && messageQueue.IsEmpty) {
                    if (!enabled) return;

                    waitEvent.Reset();
                    waitEvent.Wait(token);
                }
            }
        }

        private async Task<bool> OnProcess(CancellationToken token)
        {
            var emptyCount = BufferSize - packetSourceList.Count;

            if (emptyCount > 0) {
                for (var i = 0; i < emptyCount; i++) {
                    if (messageQueue.IsEmpty) break;

                    if (!messageQueue.TryDequeue(out var message))
                        continue;

                    var packetSource = await CreatePacketSource(message, token);
                    packetSourceList.Add(packetSource);
                }
            }

            var count = packetSourceList.Count;
            for (var i = count - 1; i >= 0; i--) {
                var packetSource = packetSourceList[i];

                var packet = await packetSource.TryTakePacket(token);

                packet?.WriteTo(writer);

                if (packetSource.IsComplete) {
                    packetSourceList.RemoveAt(i);
                    packetSource.Dispose();
                }
            }

            return count > 0;
        }

        private async Task<PacketSource> CreatePacketSource(IMessage message, CancellationToken token)
        {
            var messageId = message.MessageId;
            var messageType = message.GetType().AssemblyQualifiedName;
            var messageData = new MemoryStream();

            Stream streamData = null;
            if (message is IStreamMessage streamMessage) {
                try {
                    streamData = streamMessage.StreamFunc();
                }
                catch (Exception error) {
                    OnThreadError(new ApplicationException("Failed to open message stream source!", error));
                }
            }
            else if (message is IFileMessage fileMessage) {
                try {
                    streamData = File.Open(fileMessage.Filename, FileMode.Open, FileAccess.Read, FileShare.Read);
                }
                catch (Exception error) {
                    OnThreadError(new ApplicationException("Failed to open message file source!", error));
                }
            }

            // TODO: BsonDataWriter should be disposed!
            //   but it will close the stream.
            var bsonWriter = new BsonDataWriter(messageData);

            jsonSerializer.Serialize(bsonWriter, message);
            await bsonWriter.FlushAsync(token);

            messageData.Seek(0, SeekOrigin.Begin);

            return new PacketSource(messageId, messageType) {
                MessageData = messageData,
                StreamData = streamData,
            };
        }

        protected void OnThreadError(Exception error)
        {
            ThreadError?.Invoke(this, new ThreadExceptionEventArgs(error));
        }
    }
}
