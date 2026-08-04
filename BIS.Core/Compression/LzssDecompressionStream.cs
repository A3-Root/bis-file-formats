using System;
using System.IO;

namespace BIS.Core.Compression
{
    /// <summary>
    /// Provides forward-only, streaming decompression of an LZSS block used by BI file formats.
    /// </summary>
    public sealed class LzssDecompressionStream : Stream
    {
        private const int RingSize = 4096;
        private const int LookAheadSize = 18;
        private readonly Stream input;
        private readonly bool useSignedChecksum;
        private readonly bool leaveOpen;
        private readonly long expectedSize;
        private readonly byte[] history = new byte[RingSize];
        private long position;
        private int historyPosition = RingSize - LookAheadSize;
        private int flags;
        private int matchPosition;
        private int matchBytesRemaining;
        private int checksum;
        private bool checksumValidated;
        private Exception checksumException;
        private bool faulted;
        private bool disposed;

        /// <summary>
        /// Initializes a new LZSS decompression stream.
        /// </summary>
        /// <param name="input">The stream containing an LZSS block followed by its checksum.</param>
        /// <param name="expectedSize">The exact decompressed size of the block.</param>
        /// <param name="useSignedChecksum"><see langword="true"/> to calculate the checksum with signed bytes, as required by PAA files.</param>
        /// <param name="leaveOpen"><see langword="true"/> to leave <paramref name="input"/> open when this stream is disposed.</param>
        /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="input"/> is not readable.</exception>
        public LzssDecompressionStream(Stream input, uint expectedSize, bool useSignedChecksum = false, bool leaveOpen = false)
        {
            this.input = input ?? throw new ArgumentNullException(nameof(input));
            if (!input.CanRead)
            {
                throw new ArgumentException("The input stream must be readable.", nameof(input));
            }

            this.expectedSize = expectedSize;
            this.useSignedChecksum = useSignedChecksum;
            this.leaveOpen = leaveOpen;
            for (var i = 0; i < RingSize - LookAheadSize; i++)
            {
                history[i] = 0x20;
            }
            checksumValidated = expectedSize == 0;
        }

        /// <inheritdoc/>
        public override bool CanRead => !disposed;

        /// <inheritdoc/>
        public override bool CanSeek => false;

        /// <inheritdoc/>
        public override bool CanWrite => false;

        /// <inheritdoc/>
        public override long Length
        {
            get
            {
                ThrowIfDisposed();
                return expectedSize;
            }
        }

        /// <inheritdoc/>
        public override long Position
        {
            get
            {
                ThrowIfDisposed();
                return position;
            }
            set => throw new NotSupportedException();
        }

        /// <inheritdoc/>
        /// <param name="buffer">The buffer value.</param>
        /// <param name="offset">The offset value.</param>
        /// <param name="count">The count value.</param>
        /// <returns>The resulting value.</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            ThrowIfDisposed();
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
            if (buffer.Length - offset < count) throw new ArgumentException("The offset and count exceed the buffer bounds.");

            try
            {
                var read = 0;
                while (read < count)
                {
                    var value = ReadDecompressedByte();
                    if (value < 0) break;
                    buffer[offset + read++] = (byte)value;
                }
                return read;
            }
            catch
            {
                faulted = true;
                throw;
            }
        }

        /// <inheritdoc/>
        /// <returns>The resulting value.</returns>
        public override int ReadByte()
        {
            ThrowIfDisposed();
            try
            {
                return ReadDecompressedByte();
            }
            catch
            {
                faulted = true;
                throw;
            }
        }

        /// <inheritdoc/>
        public override void Flush()
        {
            ThrowIfDisposed();
        }

        /// <inheritdoc/>
        /// <param name="offset">The offset value.</param>
        /// <param name="origin">The origin value.</param>
        /// <returns>The resulting value.</returns>
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        /// <inheritdoc/>
        /// <param name="value">The value to process.</param>
        public override void SetLength(long value) => throw new NotSupportedException();

        /// <inheritdoc/>
        /// <param name="buffer">The buffer value.</param>
        /// <param name="offset">The offset value.</param>
        /// <param name="count">The count value.</param>
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        /// <inheritdoc/>
        /// <param name="disposing">The disposing value.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposed)
            {
                base.Dispose(disposing);
                return;
            }

            try
            {
                if (disposing && !faulted)
                {
                    DrainAndValidate();
                }
            }
            finally
            {
                disposed = true;
                if (disposing && !leaveOpen)
                {
                    input.Dispose();
                }
                base.Dispose(disposing);
            }
        }

        private int ReadDecompressedByte()
        {
            if (position == expectedSize)
            {
                ValidateChecksum();
                return -1;
            }

            int value;
            if (matchBytesRemaining > 0)
            {
                value = history[matchPosition & (RingSize - 1)];
                matchPosition++;
                matchBytesRemaining--;
            }
            else
            {
                if (((flags >>= 1) & 0x100) == 0)
                {
                    flags = ReadRequiredByte() | 0xff00;
                }

                if ((flags & 1) != 0)
                {
                    value = ReadRequiredByte();
                }
                else
                {
                    var low = ReadRequiredByte();
                    var high = ReadRequiredByte();
                    matchPosition = low | ((high & 0xf0) << 4);
                    matchBytesRemaining = (high & 0x0f) + 3;
                    if (matchBytesRemaining > expectedSize - position)
                    {
                        throw new InvalidDataException("The LZSS block expands beyond the expected output size.");
                    }
                    value = history[matchPosition & (RingSize - 1)];
                    matchPosition++;
                    matchBytesRemaining--;
                }
            }

            var output = (byte)value;
            unchecked
            {
                checksum += useSignedChecksum ? (int)(sbyte)output : output;
            }
            history[historyPosition] = output;
            historyPosition = (historyPosition + 1) & (RingSize - 1);
            position++;
            return output;
        }

        private void ValidateChecksum()
        {
            if (checksumValidated) return;
            if (checksumException != null) throw checksumException;
            try
            {
                var stored = ReadRequiredByte()
                    | (ReadRequiredByte() << 8)
                    | (ReadRequiredByte() << 16)
                    | (ReadRequiredByte() << 24);
                if (stored != checksum)
                {
                    throw new InvalidDataException("The LZSS checksum does not match the decompressed data.");
                }
                checksumValidated = true;
            }
            catch (Exception exception)
            {
                checksumException = exception;
                throw;
            }
        }

        private void DrainAndValidate()
        {
            if (checksumValidated || checksumException != null) return;
            while (ReadDecompressedByte() >= 0)
            {
            }
        }

        private int ReadRequiredByte()
        {
            var value = input.ReadByte();
            if (value < 0)
            {
                throw new EndOfStreamException("The LZSS block is truncated.");
            }
            return value;
        }

        private void ThrowIfDisposed()
        {
            if (disposed) throw new ObjectDisposedException(nameof(LzssDecompressionStream));
        }
    }
}
