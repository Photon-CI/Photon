using Photon.Communication.Messages;
using System;
using System.Net.Sockets;
using System.Threading;
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

        public MessageTransceiver Transceiver {get;}
        private readonly TaskCompletionSource<bool> handshakeResult;
        private bool isConnected;

        public TcpClient Tcp {get;}


        public MessageHost(TcpClient client, MessageProcessorRegistry registry)
        {
            this.Tcp = client;

            isConnected = true;
            handshakeResult = new TaskCompletionSource<bool>();

            Transceiver = new MessageTransceiver(registry) {
                Context = this,
            };

            Transceiver.ThreadException += Transceiver_OnThreadException;

            var stream = client.GetStream();
            Transceiver.Start(stream);
        }

        public void Dispose()
        {
            Transceiver?.Dispose();
            Tcp?.Dispose();
        }

        public void Stop(CancellationToken token = default(CancellationToken))
        {
            if (!isConnected) return;
            isConnected = false;

            handshakeResult.TrySetCanceled();

            try {
                Transceiver.Flush(token);
                Transceiver.Stop();
            }
            catch {}

            try {
                Tcp.Close();
            }
            catch {}

            OnStopped();
        }

        public void SendOneWay(IRequestMessage message)
        {
            Transceiver.SendOneWay(message);
        }

        public MessageHandle Send(IRequestMessage message)
        {
            return Transceiver.Send(message);
        }

        public async Task<bool> GetHandshakeResult(CancellationToken token)
        {
            return await handshakeResult.Task;
        }

        public void CompleteHandshake(bool result)
        {
            handshakeResult.SetResult(result);
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
