using System;
using System.IO;
using System.Threading.Tasks;

namespace Photon.Communication.Packets
{
    internal class PacketSource : IDisposable
    {
        private readonly string messageId;
        private readonly string messageType;
        private readonly Stream messageData;
        private readonly long messageSize;

        private byte[] messageDataBuffer;
        private bool isStarted;

        public bool IsComplete {get; private set;}
        public int PacketSize {get; set;}


        public PacketSource(string messageId, string messageType, Stream messageData)
        {
            this.messageId = messageId ?? throw new ArgumentNullException(nameof(messageId));
            this.messageType = messageType ?? throw new ArgumentNullException(nameof(messageType));
            this.messageData = messageData ?? throw new ArgumentNullException(nameof(messageData));

            PacketSize = 4096;
            messageDataBuffer = new byte[PacketSize];
            messageSize = messageData.Length;
        }

        public void Dispose()
        {
            messageData?.Dispose();
        }

        public async Task<IPacket> TryTakePacket()
        {
            if (!isStarted) {
                isStarted = true;
                return new HeaderPacket(messageId, messageType, messageSize);
            }

            var readSize = await messageData.ReadAsync(messageDataBuffer, 0, PacketSize);

            if (readSize > 0) {
                if (messageData.Position >= messageData.Length)
                    IsComplete = true;

                return new DataPacket(messageId, messageDataBuffer, readSize);
            }

            IsComplete = true;
            return null;
        }
    }
}
