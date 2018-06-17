using Photon.Communication.Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Communication.Packets
{
    internal class PacketReceiver : IDisposable
    {
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        private readonly NetworkStream stream;
        private readonly ManualResetEventSlim completeEvent;
        private readonly BinaryReader reader;
        private readonly Dictionary<string, PacketBuilder> packetBuilderList;

        public int Count => packetBuilderList.Count;


        public PacketReceiver(NetworkStream stream)
        {
            this.stream = stream;

            reader = new BinaryReader(stream, Encoding.UTF8, true);
            packetBuilderList = new Dictionary<string, PacketBuilder>(StringComparer.Ordinal);
            completeEvent = new ManualResetEventSlim(true);
        }

        public void Dispose()
        {
            completeEvent?.Dispose();
            reader?.Dispose();
            stream?.Dispose();
        }

        public void Stop(CancellationToken token)
        {
            try {
                completeEvent.Wait(token);
            }
            catch (OperationCanceledException) {}

            stream.Flush();
            stream.Close();
        }

        public async Task ReadPacket(CancellationToken token = default(CancellationToken))
        {
            var packet = await ParsePacket(token);
            var messageId = packet.MessageId;

            if (!packetBuilderList.TryGetValue(messageId, out var packetBuilder)) {
                packetBuilder = new PacketBuilder(messageId);
                packetBuilderList[messageId] = packetBuilder;

                // TODO: Lock Access
                completeEvent.Reset();
            }

            await packetBuilder.Append(packet);

            if (packetBuilder.IsComplete) {
                var message = packetBuilder.GetMessage();
                OnMessageReceived(message);

                packetBuilderList.Remove(messageId);
                packetBuilder.Dispose();

                // TODO: Lock Access
                if (packetBuilderList.Count == 0 && !stream.DataAvailable)
                    completeEvent.Set();
            }
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
                if (readSize == 0) throw new EndOfStreamException();

                bufferPos += readSize;
            }

            return buffer;
        }

        protected virtual void OnMessageReceived(IMessage message)
        {
            MessageReceived?.Invoke(this, new MessageReceivedEventArgs(message));
        }
    }
}
