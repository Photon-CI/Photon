using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Tests.Internal.TRx
{
    internal class DuplexerInputStream : Stream
    {
        private readonly DuplexerPipe pipe;

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;

        public override long Length => throw new NotSupportedException();

        public override long Position {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }


        public DuplexerInputStream(DuplexerPipe pipe)
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
            return pipe.Read(buffer, offset, count);
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return await pipe.ReadAsync(buffer, offset, count, cancellationToken);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
    }
}
