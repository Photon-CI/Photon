using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System;
using System.IO;
using System.Threading.Tasks;
using Photon.Communication.Messages;

namespace Photon.Communication.Packets
{
    internal class PacketBuilder : IDisposable
    {
        private readonly MemoryStream messageData;
        private readonly JsonSerializer jsonSerializer;
        private MemoryStream streamData;

        public string MessageId {get;}
        public string MessageTypeName {get; private set;}
        public long MessageLength {get; private set;}
        public long StreamLength {get; private set;}
        public bool IsComplete {get; private set;}


        public PacketBuilder(string messageId)
        {
            this.MessageId = messageId;

            messageData = new MemoryStream();
            streamData = new MemoryStream();
            jsonSerializer = new JsonSerializer();
        }

        public void Dispose()
        {
            messageData?.Dispose();
            streamData?.Dispose();
        }

        public async Task Append(IPacket packet)
        {
            if (packet is HeaderPacket headerPacket) {
                MessageTypeName = headerPacket.MessageType;
                MessageLength = headerPacket.MessageLength;
                StreamLength = headerPacket.StreamLength;
                return;
            }

            if (packet is DataPacket dataPacket) {
                if (messageData.Length < MessageLength) {
                    await messageData.WriteAsync(dataPacket.PacketBuffer, 0, dataPacket.PacketSize);
                }
                else if (streamData.Length < StreamLength) {
                    await streamData.WriteAsync(dataPacket.PacketBuffer, 0, dataPacket.PacketSize);
                }

                if (messageData.Length >= MessageLength && streamData.Length >= StreamLength)
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

            object messageObject;
            messageData.Seek(0, SeekOrigin.Begin);
            using (var bsonReader = new BsonDataReader(messageData)) {
                messageObject = jsonSerializer.Deserialize(bsonReader, messageType);
                if (messageData == null) throw new Exception("Failed to parse message!");
            }

            var message = messageObject as IMessage;
            if (message == null) throw new ApplicationException($"Invalid message type '{messageType}'!");

            if (message is IStreamMessage streamMessage) {
                streamData.Seek(0, SeekOrigin.Begin);
                var _stream = streamData;
                streamMessage.StreamFunc = () => _stream;
                streamData = null;
            }

            return message;
        }
    }
}
