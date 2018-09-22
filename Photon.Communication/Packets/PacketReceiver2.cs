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
    internal class PacketReceiver2 : IDisposable
    {
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        private readonly Stream stream;
        private readonly BinaryReader reader;
        private readonly Dictionary<string, PacketBuilder> packetBuilderList;
        private readonly ActionBlock<IMessage> messageQueue;
        private readonly CancellationTokenSource tokenSource;
        private volatile bool isActive;
        private Task _task;

        public int Count => packetBuilderList.Count;


        public PacketReceiver2(Stream stream)
        {
            this.stream = stream;

            reader = new BinaryReader(stream, Encoding.UTF8, true);
            packetBuilderList = new Dictionary<string, PacketBuilder>(StringComparer.Ordinal);
            messageQueue = new ActionBlock<IMessage>(OnMessageReceived);
            tokenSource = new CancellationTokenSource();
        }

        public void Dispose()
        {
            reader?.Dispose();
            stream?.Dispose();
            tokenSource?.Dispose();
        }

        public void Start()
        {
            if (isActive) throw new InvalidOperationException();
            isActive = true;

            _task = ReadProcess();
        }

        public void Stop()
        {
            if (!isActive) return;
            isActive = false;

            tokenSource.Cancel();

            stream.Close();
        }

        public void Flush(CancellationToken cancellationToken = default(CancellationToken))
        {
            tokenSource.Cancel();

            try {
                _task.Wait(cancellationToken);
            }
            catch (OperationCanceledException) {}

            messageQueue.Complete();
            messageQueue.Completion.Wait(cancellationToken);
        }

        private async Task ReadProcess()
        {
            var token = tokenSource.Token;

            while (true) {
                token.ThrowIfCancellationRequested();

                bool result;
                try {
                    result = await ReadPacket(token);
                }
                catch (IOException error) when (error.HResult == -2146232800 && (error.InnerException?.HResult ?? 0) == -2147467259) {
                    // Client Disconnected
                    return;
                }
                catch (OperationCanceledException) {
                    // Closed
                    return;
                }
                catch (ObjectDisposedException) {
                    // Stream Closed
                    return;
                }
                catch (EndOfStreamException) {
                    // Stream Closed
                    return;
                }
                //catch (Exception error) {
                //    OnThreadException(error);
                //}

                if (!result && !isActive && packetBuilderList.Count == 0) return;
            }
        }

        private async Task<bool> ReadPacket(CancellationToken token = default(CancellationToken))
        {
            var packet = await ParsePacket(token);
            if (packet == null) return false;

            var messageId = packet.MessageId;

            if (!packetBuilderList.TryGetValue(messageId, out var packetBuilder)) {
                packetBuilder = new PacketBuilder(messageId);
                packetBuilderList[messageId] = packetBuilder;
            }

            await packetBuilder.Append(packet);

            if (packetBuilder.IsComplete) {
                var message = packetBuilder.GetMessage();
                messageQueue.Post(message);

                packetBuilderList.Remove(messageId);
                packetBuilder.Dispose();
            }

            return true;
        }

        private async Task<IPacket> ParsePacket(CancellationToken token)
        {
            var packetHeader = await ReadBlockAsync(17, token);

            var bufferId = new byte[16];
            Array.Copy(packetHeader, 0, bufferId, 0, 16);
            var _id = new Guid(bufferId);

            var messageId = _id.ToString("N");
            var packetType = packetHeader[16];

            switch (packetType) {
                case PacketTypes.Header:
                    var headerMessageType = reader.ReadString();
                    var headerMessageSize = reader.ReadInt64();
                    var headerStreamSize = reader.ReadInt64();

                    return new HeaderPacket(messageId, headerMessageType, headerMessageSize, headerStreamSize);
                case PacketTypes.Data:
                    var dataMessageSize = reader.ReadInt32();
                    var dataMessageData = reader.ReadBytes(dataMessageSize);

                    return new DataPacket(messageId, dataMessageData, dataMessageSize);
                default:
                    throw new Exception($"Unknown packet type '{packetType}'!");
            }
        }

        private async Task<byte[]> ReadBlockAsync(int length, CancellationToken token)
        {
            var buffer = new byte[length];
            var bufferPos = 0;

            while (bufferPos < buffer.Length) {
                token.ThrowIfCancellationRequested();

                var remainingSize = buffer.Length - bufferPos;
                var readSize = await stream.ReadAsync(buffer, bufferPos, remainingSize, token);
                if (readSize == 0) {
                    throw new EndOfStreamException();
                }

                bufferPos += readSize;
            }

            return buffer;
        }

        protected virtual Task OnMessageReceived(IMessage message)
        {
            MessageReceived?.Invoke(this, new MessageReceivedEventArgs(message));
            return Task.CompletedTask;
        }
    }
}
