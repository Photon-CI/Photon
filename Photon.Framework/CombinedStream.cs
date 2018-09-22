using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Framework
{
    public class CombinedStream : Stream
    {
        private readonly Stream readableStream;
        private readonly Stream writableStream;

        public CombinedStream(Stream readableStream, Stream writableStream)
        {
            this.readableStream = readableStream ?? throw new ArgumentNullException(nameof(readableStream));
            this.writableStream = writableStream ?? throw new ArgumentNullException(nameof(writableStream));

            if (!readableStream.CanRead) throw new ArgumentException("Stream must be readable!", nameof(readableStream));
            if (!writableStream.CanWrite) throw new ArgumentException("Stream must be writable!", nameof(readableStream));
        }

        public override bool CanRead => !IsDisposed;

        public override bool CanSeek => false;

        public override bool CanWrite => !IsDisposed;

        public override bool CanTimeout => readableStream.CanTimeout || writableStream.CanTimeout;

        public override int ReadTimeout {
            get => readableStream.ReadTimeout;
            set => readableStream.ReadTimeout = value;
        }

        public override int WriteTimeout {
            get => writableStream.WriteTimeout;
            set => writableStream.WriteTimeout = value;
        }

        public override long Length => throw ThrowDisposedOr(new NotSupportedException());

        public override long Position {
            get => throw ThrowDisposedOr(new NotSupportedException());
            set => throw ThrowDisposedOr(new NotSupportedException());
        }

        public bool IsDisposed { get; private set; }

        public override long Seek(long offset, SeekOrigin origin) => throw ThrowDisposedOr(new NotSupportedException());

        public override void SetLength(long value) => ThrowDisposedOr(new NotSupportedException());

        public override void Flush() => writableStream.Flush();

        public override Task FlushAsync(CancellationToken cancellationToken) => writableStream.FlushAsync(cancellationToken);

        public override int Read(byte[] buffer, int offset, int count) => readableStream.Read(buffer, offset, count);

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => readableStream.ReadAsync(buffer, offset, count, cancellationToken);

        public override int ReadByte() => readableStream.ReadByte();

        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken) => readableStream.CopyToAsync(destination, bufferSize, cancellationToken);

        public override void Write(byte[] buffer, int offset, int count) => writableStream.Write(buffer, offset, count);

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => writableStream.WriteAsync(buffer, offset, count, cancellationToken);

        public override void WriteByte(byte value) => writableStream.WriteByte(value);

        protected override void Dispose(bool disposing)
        {
            IsDisposed = true;
            if (disposing)
            {
                readableStream.Dispose();
                writableStream.Dispose();
            }

            base.Dispose(disposing);
        }

        private Exception ThrowDisposedOr(Exception ex)
        {
            if (IsDisposed) throw new ObjectDisposedException("this");
            //Verify.NotDisposed(this);
            throw ex;
        }
    }
}
