using Photon.Communication.Packets;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Photon.Communication.Messages;

namespace Photon.Communication
{
    internal class MessageSender : IDisposable
    {
        private readonly object startStopLock;
        private PacketSender packetSender;

        private Stream stream;
        private BinaryWriter writer;
        private bool isStarted;


        public MessageSender()
        {
            startStopLock = new object();
        }

        public void Dispose()
        {
            packetSender?.Dispose();
            writer?.Dispose();
            stream?.Dispose();
        }

        public void Start(Stream stream)
        {
            lock (startStopLock) {
                if (isStarted) throw new Exception("Queue has already been started!");
                isStarted = true;
            }

            this.stream = stream;

            writer = new BinaryWriter(stream, Encoding.UTF8, true);

            packetSender = new PacketSender(writer, 4096);
            packetSender.Start();
        }

        public async Task StopAsync()
        {
            lock (startStopLock) {
                if (!isStarted) return;
                isStarted = false;
            }

            await packetSender.StopAsync();
        }

        public void Send(IMessage message)
        {
            packetSender.Enqueue(message);
        }
    }
}
