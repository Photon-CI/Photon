using System.IO;

namespace Photon.Communication.Packets
{
    internal interface IPacket
    {
        string MessageId {get;}
        byte PacketType {get;}

        void WriteTo(BinaryWriter writer);
    }
}
