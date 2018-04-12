using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Photon.Communication.Messages;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Photon.Communication.Packets
{
    internal class PacketBuilder : IDisposable
    {
        private readonly JsonSerializer jsonSerializer;
        private Stream messageData;
        private Stream streamData;
        private string tempFilename;

        public string MessageId {get;}
        public string MessageTypeName {get; private set;}
        public long MessageLength {get; private set;}
        public long StreamLength {get; private set;}
        public bool IsComplete {get; private set;}


        public PacketBuilder(string messageId)
        {
            this.MessageId = messageId;

            jsonSerializer = new JsonSerializer();
        }

        public void Dispose()
        {
            messageData?.Dispose();
            messageData = null;

            streamData?.Dispose();
            streamData = null;
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
                await AppendDataPacket(dataPacket);

                var actualMessageLength = messageData?.Length ?? 0;
                var actualDataLength = streamData?.Length ?? 0;

                if (actualMessageLength >= MessageLength && actualDataLength >= StreamLength)
                    IsComplete = true;

                return;
            }

            throw new Exception($"Unknown packet type '{packet.GetType().Name}'!");
        }

        private async Task AppendDataPacket(DataPacket dataPacket)
        {
            if (MessageLength > 0) {
                if (messageData == null)
                    messageData = new MemoryStream((int)MessageLength);

                if (messageData.Length < MessageLength) {
                    await messageData.WriteAsync(dataPacket.PacketBuffer, 0, dataPacket.PacketSize);
                    return;
                }
            }

            if (StreamLength > 0) {
                if (streamData == null) {
                    PrepareDataStream();
                }

                if (streamData.Length < StreamLength) {
                    await streamData.WriteAsync(dataPacket.PacketBuffer, 0, dataPacket.PacketSize);
                    return;
                }
            }

            throw new Exception("No target for data packet!");
        }

        private void PrepareDataStream()
        {
            var messageType = Type.GetType(MessageTypeName);

            if (typeof(IFileMessage).IsAssignableFrom(messageType)) {
                tempFilename = Path.GetTempFileName();
                streamData = File.Open(tempFilename, FileMode.Create, FileAccess.Write);
            }
            else if (typeof(IStreamMessage).IsAssignableFrom(messageType)) {
                streamData = new MemoryStream((int)Math.Min(StreamLength, int.MaxValue));
            }
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

            if (message is IFileMessage fileMessage) {
                fileMessage.Filename = tempFilename;
                streamData?.Close();
            }

            return message;
        }
    }
}
