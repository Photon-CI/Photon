using Photon.Communication.Messages;
using Photon.Communication.Packets;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Communication
{
    internal class MessageReceiver : IDisposable
    {
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;
        public event UnhandledExceptionEventHandler ThreadException;

        private volatile bool isDisposed;
        private PacketReceiver packetReceiver;
        private CancellationTokenSource tokenSource;
        private Stream stream;


        public void Dispose()
        {
            if (isDisposed) return;
            isDisposed = true;

            packetReceiver?.Dispose();
            stream?.Dispose();
        }

        public void Start(Stream stream)
        {
            this.stream = stream;

            packetReceiver = new PacketReceiver(stream);
            packetReceiver.MessageReceived += PacketReceiver_MessageReceived;

            tokenSource = new CancellationTokenSource();

            var _ = OnProcess(tokenSource.Token);
        }

        private async Task OnProcess(CancellationToken token)
        {
            while (!isDisposed && !token.IsCancellationRequested) {
                try {
                    await packetReceiver.ReadPacket(token);
                }
                catch (IOException error) when (error.HResult == -2146232800 && (error.InnerException?.HResult ?? 0) == -2147467259) {
                    // Client Disconnected
                    return;
                }
                catch (ObjectDisposedException) {
                    // Stream Closed
                    return;
                }
                catch (EndOfStreamException) {
                    // Stream Closed
                    return;
                }
                catch (Exception error) {
                    OnThreadException(error);
                }
            }
        }

        public void Stop(CancellationToken token = default(CancellationToken))
        {
            tokenSource.Cancel();

            try {
                packetReceiver.Stop(token);
            }
            catch {}
        }

        private void PacketReceiver_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            OnMessageReceived(e.Message);
        }

        protected virtual void OnMessageReceived(IMessage message)
        {
            // TODO: Track Tasks
            Task.Run(() => {
                MessageReceived?.Invoke(this, new MessageReceivedEventArgs(message));
            });
        }

        protected virtual void OnThreadException(object exceptionObject)
        {
            ThreadException?.Invoke(this, new UnhandledExceptionEventArgs(exceptionObject, false));
        }
    }
}
