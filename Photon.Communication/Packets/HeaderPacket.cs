using System.IO;

namespace Photon.Communication.Packets
{
    internal class HeaderPacket : IPacket
    {
        public string MessageId {get;}
        public byte PacketType => PacketTypes.Header;
        public string MessageType {get;}
        public long MessageLength {get;}
        public long StreamLength {get;}


        public HeaderPacket(string messageId, string messageType, long messageLength, long streamLength)
        {
            this.MessageId = messageId;
            this.MessageType = messageType;
            this.MessageLength = messageLength;
            this.StreamLength = streamLength;
        }

        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(MessageId);
            writer.Write(PacketType);
            writer.Write(MessageType);
            writer.Write(MessageLength);
            writer.Write(StreamLength);
        }
    }
}
