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

        private PacketReceiver packetReceiver;
        private CancellationTokenSource tokenSource;
        private Stream stream;
        private Task task;


        public void Dispose()
        {
            packetReceiver?.Dispose();
            stream?.Dispose();
        }

        public void Start(Stream stream)
        {
            this.stream = stream;

            packetReceiver = new PacketReceiver(stream);
            packetReceiver.MessageReceived += PacketReceiver_MessageReceived;

            tokenSource = new CancellationTokenSource();

            task = Task.Run(async () => {
                while (!tokenSource.IsCancellationRequested) {
                    try {
                        await packetReceiver.ReadPacket();
                    }
                    catch (IOException error) when (error.HResult == -2146232800 && (error.InnerException?.HResult ?? 0) == -2147467259) {
                        // Client Disconnected
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
            });
        }

        public async Task StopAsync()
        {
            tokenSource.Cancel();
            try {
                stream.Close();
                await task;
            }
            catch {}
        }

        private void PacketReceiver_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            OnMessageReceived(e.Message);
        }

        protected virtual void OnMessageReceived(IMessage message)
        {
            MessageReceived?.Invoke(this, new MessageReceivedEventArgs(message));
        }

        protected virtual void OnThreadException(object exceptionObject)
        {
            ThreadException?.Invoke(this, new UnhandledExceptionEventArgs(exceptionObject, false));
        }
    }
}
