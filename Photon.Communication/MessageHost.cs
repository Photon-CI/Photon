using Photon.Communication.Messages;
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
        public event UnhandledExceptionEventHandler ThreadException;

        public event EventHandler Stopped;

        private readonly TcpClient client;
        private readonly MessageTransceiver transceiver;
        private bool isConnected;


        public MessageHost(TcpClient client, MessageProcessorRegistry registry)
        {
            this.client = client;

            isConnected = true;
            transceiver = new MessageTransceiver(registry);
            transceiver.ThreadException += Transceiver_OnThreadException;

            transceiver.Start(client);
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

        protected virtual void OnThreadException(object exceptionObject)
        {
            ThreadException?.Invoke(this, new UnhandledExceptionEventArgs(exceptionObject, false));
        }

        private void Transceiver_OnThreadException(object sender, UnhandledExceptionEventArgs e)
        {
            OnThreadException(e.ExceptionObject);
        }
    }
}
