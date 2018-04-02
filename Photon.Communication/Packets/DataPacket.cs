using System.IO;
using System.Threading.Tasks;

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

        public async Task WriteToAsync(BinaryWriter writer)
        {
            await Task.Run(() => {
                writer.Write(MessageId);
                writer.Write(PacketType);
                writer.Write(PacketSize);
                writer.Write(PacketBuffer, 0, PacketSize);
            });
        }

        // TODO: Parse Method
    }
}
