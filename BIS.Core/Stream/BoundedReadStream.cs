using System;
using System.IO;

namespace BIS.Core.Streams
{
    internal sealed class BoundedReadStream : Stream
    {
        private readonly Stream input;
        private readonly bool leaveOpen;
        private readonly long length;
        private long position;
        private bool faulted;
        private bool disposed;

        internal BoundedReadStream(Stream input, uint length, bool leaveOpen)
        {
            this.input = input ?? throw new ArgumentNullException(nameof(input));
            this.length = length;
            this.leaveOpen = leaveOpen;
        }

        public override bool CanRead => !disposed;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => length;
        public override long Position
        {
            get => position;
            set => throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (disposed) throw new ObjectDisposedException(nameof(BoundedReadStream));
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
            if (buffer.Length - offset < count) throw new ArgumentException("The offset and count exceed the buffer bounds.");
            count = (int)System.Math.Min(count, length - position);
            if (count == 0) return 0;
            try
            {
                var read = input.Read(buffer, offset, count);
                if (read == 0) throw new EndOfStreamException("The uncompressed block is truncated.");
                position += read;
                return read;
            }
            catch
            {
                faulted = true;
                throw;
            }
        }

        public override int ReadByte()
        {
            if (position == length) return -1;
            try
            {
                var value = input.ReadByte();
                if (value < 0) throw new EndOfStreamException("The uncompressed block is truncated.");
                position++;
                return value;
            }
            catch
            {
                faulted = true;
                throw;
            }
        }

        public override void Flush() { }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            if (!disposed && disposing)
            {
                try
                {
                    var buffer = new byte[4096];
                    while (!faulted && position < length)
                    {
                        Read(buffer, 0, (int)System.Math.Min(buffer.Length, length - position));
                    }
                }
                finally
                {
                    if (!leaveOpen) input.Dispose();
                }
            }
            disposed = true;
            base.Dispose(disposing);
        }
    }
}
