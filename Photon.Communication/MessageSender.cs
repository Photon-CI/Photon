using Photon.Communication.Messages;
using Photon.Communication.Packets;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Communication
{
    internal class MessageSender : IDisposable
    {
        public event ThreadExceptionEventHandler ThreadError;

        private readonly object startStopLock;
        private PacketSender packetSender;

        private Stream stream;
        private bool isStarted;


        public MessageSender()
        {
            startStopLock = new object();
        }

        public void Dispose()
        {
            packetSender?.Dispose();
            stream?.Dispose();
        }

        public void Start(Stream stream)
        {
            lock (startStopLock) {
                if (isStarted) throw new Exception("Queue has already been started!");
                isStarted = true;
            }

            this.stream = stream;

            packetSender = new PacketSender(stream, Encoding.UTF8, 4096);
            packetSender.ThreadError += PacketSender_ThreadError;
            packetSender.Start();
        }

        public void Flush(CancellationToken cancellationToken = default(CancellationToken))
        {
            packetSender.Flush(cancellationToken);
        }

        public async Task FlushAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            await packetSender.FlushAsync(cancellationToken);
        }

        public void Stop()
        {
            lock (startStopLock) {
                if (!isStarted) return;
                isStarted = false;
            }

            packetSender.Stop();
        }

        public void Send(IMessage message)
        {
            packetSender.Enqueue(message);
        }

        protected void OnThreadError(Exception error)
        {
            ThreadError?.Invoke(this, new ThreadExceptionEventArgs(error));
        }

        private void PacketSender_ThreadError(object sender, ThreadExceptionEventArgs e)
        {
            OnThreadError(e.Exception);
        }
    }
}
