using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Photon.Communication.Packets
{
    internal class PacketReceiver : IDisposable
    {
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        private readonly BufferedStream streamBuffer;
        private readonly BinaryReader reader;
        private readonly Dictionary<string, PacketBuilder> packetBuilderList;


        public PacketReceiver(Stream stream)
        {
            streamBuffer = new BufferedStream(stream);
            reader = new BinaryReader(stream, Encoding.UTF8, true);
            packetBuilderList = new Dictionary<string, PacketBuilder>(StringComparer.Ordinal);
        }

        public void Dispose()
        {
            reader?.Dispose();
            //streamBuffer?.Dispose();
        }

        public async Task ReadPacket()
        {
            var packet = ParsePacket();
            var messageId = packet.MessageId;

            if (!packetBuilderList.TryGetValue(messageId, out var packetBuilder)) {
                packetBuilder = new PacketBuilder(messageId);
                packetBuilderList[messageId] = packetBuilder;
            }

            await packetBuilder.Append(packet);

            if (packetBuilder.IsComplete) {
                var message = packetBuilder.GetMessage();
                OnMessageReceived(message);

                packetBuilderList.Remove(messageId);
                packetBuilder.Dispose();
            }
        }

        private IPacket ParsePacket()
        {
            var messageId = reader.ReadString();
            var packetType = reader.ReadByte();

            switch (packetType) {
                case PacketTypes.Header:
                    var headerMessageType = reader.ReadString();
                    var headerMessageSize = reader.ReadInt64();

                    return new HeaderPacket(messageId, headerMessageType, headerMessageSize);
                case PacketTypes.Data:
                    var dataMessageSize = reader.ReadInt32();
                    var dataMessageData = reader.ReadBytes(dataMessageSize);

                    return new DataPacket(messageId, dataMessageData, dataMessageSize);
                default:
                    throw new Exception($"Unknown packet type '{packetType}'!");
            }
        }

        protected virtual void OnMessageReceived(IMessage message)
        {
            MessageReceived?.Invoke(this, new MessageReceivedEventArgs(message));
        }
    }
}
