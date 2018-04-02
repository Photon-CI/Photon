using System.IO;
using System.Threading.Tasks;

namespace Photon.Communication.Packets
{
    internal interface IPacket
    {
        string MessageId {get;}
        byte PacketType {get;}

        Task WriteToAsync(BinaryWriter writer);
    }
}
