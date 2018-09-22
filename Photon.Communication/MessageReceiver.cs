using Photon.Communication.Messages;
using Photon.Communication.Packets;
using System;
using System.IO;
using System.Threading;

namespace Photon.Communication
{
    internal class MessageReceiver : IDisposable
    {
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;
        //public event UnhandledExceptionEventHandler ThreadException;

        private volatile bool isDisposed;
        private PacketReceiver2 packetReceiver;
        //private CancellationTokenSource tokenSource;
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

            packetReceiver = new PacketReceiver2(stream);
            packetReceiver.MessageReceived += PacketReceiver_MessageReceived;

            //tokenSource = new CancellationTokenSource();

            //var _ = OnProcess(tokenSource.Token);
            packetReceiver.Start();
        }

        //private async Task OnProcess(CancellationToken token)
        //{
        //    while (true) {
        //        token.ThrowIfCancellationRequested();

        //        try {
        //            await packetReceiver.ReadPacket(token);
        //        }
        //        catch (IOException error) when (error.HResult == -2146232800 && (error.InnerException?.HResult ?? 0) == -2147467259) {
        //            // Client Disconnected
        //            return;
        //        }
        //        catch (ObjectDisposedException) {
        //            // Stream Closed
        //            return;
        //        }
        //        catch (EndOfStreamException) {
        //            // Stream Closed
        //            return;
        //        }
        //        catch (Exception error) {
        //            OnThreadException(error);
        //        }
        //    }
        //}

        public void Flush(CancellationToken cancellationToken = default(CancellationToken))
        {
            packetReceiver.Flush(cancellationToken);
        }

        //public async Task FlushAsync(CancellationToken cancellationToken = default(CancellationToken))
        //{
        //    await Task.Run(() => {
        //        packetReceiver.Flush(cancellationToken);
        //    }, cancellationToken);
        //}

        public void Stop()
        {
            //tokenSource.Cancel();

            packetReceiver.Stop();
        }

        private void PacketReceiver_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            OnMessageReceived(e.Message);
        }

        protected virtual void OnMessageReceived(IMessage message)
        {
            // TODO: Track Tasks
            MessageReceived?.Invoke(this, new MessageReceivedEventArgs(message));
        }

        //protected virtual void OnThreadException(object exceptionObject)
        //{
        //    ThreadException?.Invoke(this, new UnhandledExceptionEventArgs(exceptionObject, false));
        //}
    }
}
