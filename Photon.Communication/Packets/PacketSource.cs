using System;
using System.IO;
using System.Threading.Tasks;

namespace Photon.Communication.Packets
{
    internal class PacketSource : IDisposable
    {
        private readonly string messageId;
        private readonly string messageType;
        private readonly byte[] messageDataBuffer;

        private bool sentHeader;
        private bool sentMessage;
        private bool sentStream;
        private long messageLength;
        private long streamLength;

        public Stream MessageData {get; set;}
        public Stream StreamData {get; set;}
        public bool IsComplete {get; private set;}
        public int PacketSize {get; set;}


        public PacketSource(string messageId, string messageType)
        {
            this.messageId = messageId ?? throw new ArgumentNullException(nameof(messageId));
            this.messageType = messageType ?? throw new ArgumentNullException(nameof(messageType));

            PacketSize = 4096;
            messageDataBuffer = new byte[PacketSize];
        }

        public void Dispose()
        {
            MessageData?.Dispose();
        }

        public async Task<IPacket> TryTakePacket()
        {
            if (!sentHeader) {
                sentHeader = true;
                messageLength = MessageData?.Length ?? 0;
                streamLength = StreamData?.Length ?? 0;

                MessageData?.Seek(0, SeekOrigin.Begin);
                StreamData?.Seek(0, SeekOrigin.Begin);

                return new HeaderPacket(messageId, messageType, messageLength, streamLength);
            }

            if (MessageData != null && !sentMessage) {
                var readSize = await MessageData.ReadAsync(messageDataBuffer, 0, PacketSize);

                if (readSize > 0)
                    return new DataPacket(messageId, messageDataBuffer, readSize);

                sentMessage = true;
            }

            if (StreamData != null && !sentStream) {
                var readSize = await StreamData.ReadAsync(messageDataBuffer, 0, PacketSize);

                if (readSize > 0)
                    return new DataPacket(messageId, messageDataBuffer, readSize);

                sentStream = true;
            }

            IsComplete = true;
            return null;
        }
    }
}
