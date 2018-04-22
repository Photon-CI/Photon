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

        private readonly TcpClient client;
        private readonly MessageProcessorRegistry messageRegistry;

        public object Context {get; set;}
        public MessageTransceiver Transceiver {get; private set;}

        public bool IsConnected => Transceiver?.IsStarted ?? false;


        public MessageClient(MessageProcessorRegistry registry)
        {
            messageRegistry = registry;

            client = new TcpClient {
                NoDelay = true,
                ExclusiveAddressUse = false,
                Client = {
                    NoDelay = true,
                    ExclusiveAddressUse = false
                },
            };
        }

        public void Dispose()
        {
            Transceiver?.Dispose();
            client?.Dispose();
        }

        public async Task ConnectAsync(string hostname, int port, CancellationToken token)
        {
            token.Register(() => client.Close());
            await client.ConnectAsync(hostname, port);

            Transceiver = new MessageTransceiver(messageRegistry) {
                Context = Context,
            };
            Transceiver.ThreadException += Transceiver_OnThreadException;

            Transceiver.Start(client);
        }

        public async Task DisconnectAsync()
        {
            await Transceiver.StopAsync();
            client.Close();
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
