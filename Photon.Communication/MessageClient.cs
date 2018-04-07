using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Photon.Communication
{
    /// <inheritdoc />
    /// <summary>
    /// Connects to a remote MessageHost.
    /// </summary>
    public class MessageClient : IDisposable
    {
        private readonly TcpClient client;
        private readonly MessageRegistry messageRegistry;
        private MessageTransceiver transceiver;

        public bool IsConnected => transceiver?.IsStarted ?? false;


        public MessageClient(MessageRegistry registry)
        {
            messageRegistry = registry;

            client = new TcpClient();
        }

        public void Dispose()
        {
            transceiver?.Dispose();
            client?.Dispose();
        }

        public async Task ConnectAsync(string hostname, int port)
        {
            await client.ConnectAsync(hostname, port);

            transceiver = new MessageTransceiver(messageRegistry);
            transceiver.Start(client);
        }

        public async Task DisconnectAsync()
        {
            await transceiver.StopAsync();
            client.Close();
        }

        public MessageHandle Send(IRequestMessage message)
        {
            return transceiver.Send(message);
        }

        public void SendOneWay(IRequestMessage message)
        {
            transceiver.SendOneWay(message);
        }
    }
}
