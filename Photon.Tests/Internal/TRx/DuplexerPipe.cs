using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Photon.Tests.Internal.TRx
{
    internal class DuplexerPipe
    {
        private readonly BufferBlock<byte[]> queue;
        private byte[] currentItem;
        private int currentIndex;

        public int Count => queue?.Count ?? 0;


        public DuplexerPipe()
        {
            queue = new BufferBlock<byte[]>();
        }

        public void Flush(CancellationToken cancellationToken = default(CancellationToken))
        {
            //
        }

        public Task FlushAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.CompletedTask;
        }
       
        public void Write(byte[] buffer, int offset, int count)
        {
            var data = new byte[count];
            Array.Copy(buffer, offset, data, 0, count);
            queue.Post(data);
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            if (currentItem == null) {
                try {
                    currentItem = queue.Receive();
                    currentIndex = 0;
                }
                catch (InvalidOperationException) {
                    return 0;
                }

                if (currentItem == null) {
                    // End of Stream
                    return 0;
                }
            }

            return ReadCurrent(buffer, offset, count);
        }

        public async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (currentItem == null) {
                try {
                    currentItem = await queue.ReceiveAsync(cancellationToken);
                    currentIndex = 0;
                }
                catch (InvalidOperationException) {
                    return 0;
                }

                if (currentItem == null) {
                    // End of Stream
                    return 0;
                }
            }

            return ReadCurrent(buffer, offset, count);
        }

        private int ReadCurrent(byte[] buffer, int offset, int count)
        {
            var len = Math.Min(currentItem.Length - currentIndex, count);

            Array.Copy(currentItem, currentIndex, buffer, offset, len);
            currentIndex += len;

            if (currentIndex >= currentItem.Length) {
                currentItem = null;
            }

            return len;
        }
    }
}
