using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Photon.Communication.Packets
{
    internal class PacketBuilder : IDisposable
    {
        private readonly MemoryStream messageData;
        private readonly JsonSerializer jsonSerializer;

        public string MessageId {get;}
        public string MessageTypeName {get; private set;}
        public long MessageSize {get; private set;}
        public bool IsComplete {get; private set;}


        public PacketBuilder(string messageId)
        {
            this.MessageId = messageId;

            messageData = new MemoryStream();
            jsonSerializer = new JsonSerializer();
        }

        public void Dispose()
        {
            messageData?.Dispose();
        }

        public async Task Append(IPacket packet)
        {
            if (packet is HeaderPacket headerPacket) {
                MessageTypeName = headerPacket.MessageType;
                MessageSize = headerPacket.MessageSize;
                return;
            }

            if (packet is DataPacket dataPacket) {
                await messageData.WriteAsync(dataPacket.PacketBuffer, 0, dataPacket.PacketSize);

                if (messageData.Length >= MessageSize)
                    IsComplete = true;

                return;
            }

            throw new Exception($"Unknown packet type '{packet.GetType().Name}'!");
        }

        public IMessage GetMessage()
        {
            if (!IsComplete) throw new Exception("Message is incomplete!");

            var messageType = Type.GetType(MessageTypeName);
            if (messageType == null) throw new Exception($"Unknown message type '{MessageTypeName}'!");

            object message;
            messageData.Seek(0, SeekOrigin.Begin);
            using (var bsonReader = new BsonDataReader(messageData)) {
                message = jsonSerializer.Deserialize(bsonReader, messageType);
                if (messageData == null) throw new Exception("Failed to parse message!");
            }

            return message as IMessage;
        }
    }
}
