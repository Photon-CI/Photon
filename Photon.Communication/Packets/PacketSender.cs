using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Photon.Communication.Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Photon.Communication.Packets
{
    internal class PacketSender : IDisposable
    {
        public event ThreadExceptionEventHandler ThreadError;

        private readonly BinaryWriter writer;
        private readonly JsonSerializer jsonSerializer;
        private CancellationTokenSource tokenSource;
        private Task task;

        private BufferBlock<IMessage> queue;
        private readonly List<PacketSource> packetSourceList;

        public int BufferSize {get;}


        public PacketSender(Stream stream, Encoding encoding, int bufferSize)
        {
            this.BufferSize = bufferSize;

            if (bufferSize <= 0) throw new ArgumentOutOfRangeException(nameof(bufferSize), "Value must be greater than zero!");

            BufferSize = 4;
            packetSourceList = new List<PacketSource>();
            jsonSerializer = new JsonSerializer();
            writer = new BinaryWriter(stream, encoding, true);
        }

        public void Dispose()
        {
            Stop();

            tokenSource?.Dispose();

            foreach (var packetSource in packetSourceList)
                packetSource.Dispose();

            packetSourceList.Clear();
        }

        public void Start()
        {
            tokenSource = new CancellationTokenSource();

            queue = new BufferBlock<IMessage>();

            task = Process(tokenSource.Token);
        }

        public void Stop()
        {
            try {
                tokenSource?.Cancel();
            }
            catch {}

            try {
                writer?.Dispose();
            }
            catch {}
        }

        public void Flush(CancellationToken token = default(CancellationToken))
        {
            queue.Complete();
            queue.Completion.Wait(token);

            task.Wait(token);
        }

        public async Task FlushAsync(CancellationToken token = default(CancellationToken))
        {
            queue.Complete();

            await Task.Run(async () => {
                await queue.Completion;
                await task;
            }, token);
        }

        public void Enqueue(IMessage message)
        {
            queue.Post(message);
        }

        private async Task Process(CancellationToken token)
        {
            while (await queue.OutputAvailableAsync(token)) {
                var firstRun = true;
                while (firstRun || packetSourceList.Count > 0) {
                    token.ThrowIfCancellationRequested();
                    firstRun = false;

                    var emptyCount = BufferSize - packetSourceList.Count;

                    if (emptyCount > 0) {
                        for (var i = 0; i < emptyCount; i++) {
                            if (!queue.TryReceive(null, out var message)) break;

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
                }
            }
        }

        private async Task<PacketSource> CreatePacketSource(IMessage message, CancellationToken token)
        {
            var messageId = message.MessageId;
            var messageType = message.GetType().AssemblyQualifiedName;
            var messageData = new MemoryStream();

            Stream streamData = null;
            switch (message) {
                case IStreamMessage streamMessage:
                    try {
                        streamData = streamMessage.StreamFunc();
                    }
                    catch (Exception error) {
                        OnThreadError(new ApplicationException("Failed to open message stream source!", error));
                    }

                    break;
                case IFileMessage fileMessage:
                    try {
                        streamData = File.Open(fileMessage.Filename, FileMode.Open, FileAccess.Read, FileShare.Read);
                    }
                    catch (Exception error) {
                        OnThreadError(new ApplicationException("Failed to open message file source!", error));
                    }

                    break;
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
