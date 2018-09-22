using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Tests.Internal.TRx
{
    internal class DuplexerOutputStream : Stream
    {
        private readonly DuplexerPipe pipe;

        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;

        public override long Length => throw new NotSupportedException();

        public override long Position {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }


        public DuplexerOutputStream(DuplexerPipe pipe)
        {
            this.pipe = pipe;
        }

        public override void Flush()
        {
            pipe.Flush();
        }

        public override async Task FlushAsync(CancellationToken cancellationToken)
        {
            await pipe.FlushAsync(cancellationToken);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            pipe.Write(buffer, offset, count);
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            await Task.Run(() => pipe.Write(buffer, offset, count), cancellationToken);
        }
    }
}
