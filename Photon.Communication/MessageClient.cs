using Photon.Communication.Messages;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Communication
{
    /// <inheritdoc />
    /// <summary>
    /// Connects to a remote MessageHost.
    /// </summary>
    public class MessageClient : IDisposable
    {
        public event UnhandledExceptionEventHandler ThreadException;

        public TcpClient Tcp {get;}
        public MessageTransceiver Transceiver {get;}

        public bool IsConnected => Transceiver?.IsStarted ?? false;


        public MessageClient(MessageProcessorRegistry registry)
        {
            Tcp = new TcpClient {
                NoDelay = true,
                ExclusiveAddressUse = false,
                Client = {
                    NoDelay = true,
                    ExclusiveAddressUse = false
                },
            };

            Transceiver = new MessageTransceiver(registry) {
                Context = this,
            };

            Transceiver.ThreadException += Transceiver_OnThreadException;
        }

        public void Dispose()
        {
            Transceiver?.Dispose();
            Tcp?.Dispose();
        }

        public async Task ConnectAsync(string hostname, int port, CancellationToken token)
        {
            token.Register(() => Tcp.Close());
            await Tcp.ConnectAsync(hostname, port);

            Transceiver.Start(Tcp);
        }

        public async Task DisconnectAsync()
        {
            await Transceiver.StopAsync();
            Tcp.Close();
        }

        public async Task<TResponse> Handshake<TResponse>(IRequestMessage handshakeRequest, TimeSpan timeout, CancellationToken token)
            where TResponse : class, IResponseMessage
        {
            using (var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(token)) {
                tokenSource.CancelAfter(timeout);

                return await Send(handshakeRequest)
                    .GetResponseAsync<TResponse>(tokenSource.Token);
            }
        }

        public MessageHandle Send(IRequestMessage message)
        {
            return Transceiver.Send(message);
        }

        public void SendOneWay(IRequestMessage message)
        {
            Transceiver.SendOneWay(message);
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
