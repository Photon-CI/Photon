using System;
using System.IO;

namespace Photon.Communication.Packets
{
    internal class DataPacket : IPacket
    {
        public string MessageId {get;}
        public byte PacketType => PacketTypes.Data;
        public int PacketSize {get;}
        public byte[] PacketBuffer {get;}


        public DataPacket(string messageId, byte[] buffer, int size)
        {
            this.MessageId = messageId;

            PacketBuffer = buffer;
            PacketSize = size;
        }

        public void WriteTo(BinaryWriter writer)
        {
            var _id = Guid.ParseExact(MessageId, "N");
            var bufferId = _id.ToByteArray();

            writer.Write(bufferId);
            writer.Write(PacketType);
            writer.Write(PacketSize);
            writer.Write(PacketBuffer, 0, PacketSize);
        }
    }
}
