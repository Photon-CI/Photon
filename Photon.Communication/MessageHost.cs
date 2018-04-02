using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Photon.Communication
{
    /// <summary>
    /// An incomming TCP message connection.
    /// </summary>
    public class MessageHost
    {
        public event EventHandler Stopped;

        private readonly TcpClient client;
        private readonly MessageProcessor processor;
        private readonly MessageTransceiver transceiver;
        private bool isConnected;


        public MessageHost(TcpClient client, MessageProcessor processor)
        {
            this.client = client;
            this.processor = processor;

            transceiver = new MessageTransceiver(processor);

            var stream = client.GetStream();
            transceiver.Start(stream);
            isConnected = true;
        }

        public void Dispose()
        {
            transceiver?.Dispose();
            client?.Dispose();
        }

        public async Task StopAsync()
        {
            if (!isConnected) return;
            isConnected = false;

            try {
                await transceiver.StopAsync();
                await processor.StopAsync();

                client.Close();
            }
            finally {
                OnStopped();
            }
        }

        public void SendOneWay(IRequestMessage message)
        {
            transceiver.SendOneWay(message);
        }

        public MessageHandle Send(IRequestMessage message)
        {
            return transceiver.Send(message);
        }

        protected virtual void OnStopped()
        {
            Stopped?.Invoke(this, EventArgs.Empty);
        }
    }
}
