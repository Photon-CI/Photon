using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Photon.Framework.Communication
{
    /// <summary>
    /// Connects to a remote MessageHost.
    /// </summary>
    public class MessageClient : IDisposable
    {
        private readonly TcpClient client;
        private readonly MessageTransceiver transceiver;
        //private readonly MessageProcessor processor;


        public MessageClient(MessageProcessor processor)
        {
            //this.processor = processor;

            client = new TcpClient();
            transceiver = new MessageTransceiver(processor);
        }

        public void Dispose()
        {
            transceiver?.Dispose();
            client?.Dispose();
        }

        public async Task ConnectAsync(string hostname, int port)
        {
            await client.ConnectAsync(hostname, port);
            var stream = client.GetStream();

            transceiver.Start(stream);
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
