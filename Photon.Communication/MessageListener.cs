using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Photon.Communication
{
    /// <inheritdoc />
    /// <summary>
    /// Listens for incomming TCP message connections, and manages
    /// a collection of <see cref="T:Photon.Communication.MessageHost" /> instances.
    /// </summary>
    public class MessageListener : IDisposable
    {
        public event EventHandler<TcpConnectionReceivedEventArgs> ConnectionReceived;
        public event UnhandledExceptionEventHandler ThreadException;

        private readonly MessageProcessorRegistry messageRegistry;
        private readonly List<MessageHost> hostList;
        private readonly object startStopLock;
        private TcpListener listener;
        private bool isListening;


        public MessageListener(MessageProcessorRegistry registry)
        {
            messageRegistry = registry;

            hostList = new List<MessageHost>();
            startStopLock = new object();
        }

        public void Dispose()
        {
            foreach (var host in hostList) {
                try {
                    host.Dispose();
                }
                catch {}
            }
            hostList.Clear();

            listener?.Stop();
        }

        public void Listen(IPAddress address, int port)
        {
            lock (startStopLock) {
                if (isListening) throw new Exception("Host is already listening!");
                isListening = true;
            }

            listener = new TcpListener(address, port) {
                ExclusiveAddressUse = false,
                Server = {
                    NoDelay = true,
                    ExclusiveAddressUse = false,
                }
            };

            listener.AllowNatTraversal(true);
            listener.Start();

            listener.BeginAcceptTcpClient(Listener_OnConnectionReceived, new object());
        }

        public async Task StopAsync()
        {
            lock (startStopLock) {
                if (!isListening) return;
                isListening = false;
            }

            listener.Stop();

            var tasks = hostList.Select(x => x.StopAsync());
            await Task.WhenAll(tasks.ToArray());
        }

        private void Listener_OnConnectionReceived(IAsyncResult result)
        {
            if (!isListening) return;

            TcpClient client;
            try {
                client = listener.EndAcceptTcpClient(result);
                OnConnectionReceived(client);
            }
            catch (Exception error) {
                OnThreadException(error);
                return;
            }
            finally {
                listener.BeginAcceptTcpClient(Listener_OnConnectionReceived, new object());
            }

            var host = new MessageHost(client, messageRegistry);
            host.ThreadException += Host_OnThreadException;
            host.Stopped += Host_Stopped;
            hostList.Add(host);
        }

        private void Host_OnThreadException(object sender, UnhandledExceptionEventArgs e)
        {
            OnThreadException(e.ExceptionObject);
        }

        private void Host_Stopped(object sender, EventArgs e)
        {
            var host = (MessageHost)sender;
            hostList.Remove(host);
            host.Dispose();
        }

        protected virtual void OnConnectionReceived(TcpClient client)
        {
            var remote = (IPEndPoint)client.Client.RemoteEndPoint;
            ConnectionReceived?.Invoke(this, new TcpConnectionReceivedEventArgs(remote.Address.ToString(), remote.Port));
        }

        protected virtual void OnThreadException(object exceptionObject)
        {
            ThreadException?.Invoke(this, new UnhandledExceptionEventArgs(exceptionObject, false));
        }
    }

    public class TcpConnectionReceivedEventArgs : EventArgs
    {
        public string Host {get;}
        public int Port {get;}

        public TcpConnectionReceivedEventArgs(string host, int port)
        {
            this.Host = host;
            this.Port = port;
        }
    }
}
