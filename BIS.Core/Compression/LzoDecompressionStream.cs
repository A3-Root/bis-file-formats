using System;
using System.IO;

namespace BIS.Core.Compression
{
    /// <summary>
    /// Provides forward-only, streaming decompression of a raw LZO1X block used by BI file formats.
    /// </summary>
    public sealed class LzoDecompressionStream : Stream
    {
        private const int HistorySize = 65536;
        private const uint M2MaxOffset = 0x0800;

        private readonly Stream input;
        private readonly bool leaveOpen;
        private readonly long expectedSize;
        private readonly byte[] history = new byte[HistorySize];
        private long position;
        private long matchPosition;
        private uint pendingCount;
        private bool pendingLiterals;
        private ParserState continuation = ParserState.Start;
        private uint trailingLiteralCount;
        private bool finished;
        private bool faulted;
        private bool disposed;

        private enum ParserState
        {
            Start,
            MainToken,
            FirstLiteralRun,
            AfterMatch,
            AfterTrailingLiterals
        }

        /// <summary>
        /// Initializes a new LZO decompression stream.
        /// </summary>
        /// <param name="input">The stream containing a raw LZO1X block.</param>
        /// <param name="expectedSize">The exact decompressed size of the block.</param>
        /// <param name="leaveOpen"><see langword="true"/> to leave <paramref name="input"/> open when this stream is disposed.</param>
        /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="input"/> is not readable.</exception>
        public LzoDecompressionStream(Stream input, uint expectedSize, bool leaveOpen = false)
        {
            this.input = input ?? throw new ArgumentNullException(nameof(input));
            if (!input.CanRead)
            {
                throw new ArgumentException("The input stream must be readable.", nameof(input));
            }

            this.expectedSize = expectedSize;
            this.leaveOpen = leaveOpen;
            finished = expectedSize == 0;
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
                    DrainAndFinish();
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
                FinishBlock();
                return -1;
            }

            while (pendingCount == 0)
            {
                PrepareSegment();
                if (finished)
                {
                    throw new InvalidDataException("The LZO block ended before the expected output size was reached.");
                }
            }

            byte value;
            if (pendingLiterals)
            {
                value = ReadRequiredByte();
            }
            else
            {
                if (matchPosition < 0 || position - matchPosition > HistorySize)
                {
                    throw new InvalidDataException("The LZO block contains an invalid look-behind reference.");
                }
                value = history[(int)(matchPosition & (HistorySize - 1))];
                matchPosition++;
            }

            pendingCount--;
            history[(int)(position & (HistorySize - 1))] = value;
            position++;
            return value;
        }

        private void PrepareSegment()
        {
            while (pendingCount == 0 && !finished)
            {
                switch (continuation)
                {
                    case ParserState.Start:
                        var first = ReadRequiredByte();
                        if (first > 17)
                        {
                            var count = (uint)(first - 17);
                            if (count < 4)
                            {
                                SetLiterals(count, ParserState.AfterTrailingLiterals);
                            }
                            else
                            {
                                SetLiterals(count, ParserState.FirstLiteralRun);
                            }
                        }
                        else
                        {
                            ProcessMainToken(first);
                        }
                        break;

                    case ParserState.MainToken:
                        ProcessMainToken(ReadRequiredByte());
                        break;

                    case ParserState.FirstLiteralRun:
                        var token = ReadRequiredByte();
                        if (token >= 16)
                        {
                            ProcessMatchToken(token);
                        }
                        else
                        {
                            var offset = 1L + M2MaxOffset + (token >> 2) + (ReadRequiredByte() << 2);
                            trailingLiteralCount = (uint)(token & 3);
                            SetMatch(offset, 3);
                        }
                        break;

                    case ParserState.AfterMatch:
                        if (trailingLiteralCount == 0)
                        {
                            continuation = ParserState.MainToken;
                        }
                        else
                        {
                            SetLiterals(trailingLiteralCount, ParserState.AfterTrailingLiterals);
                        }
                        break;

                    case ParserState.AfterTrailingLiterals:
                        ProcessMatchToken(ReadRequiredByte());
                        break;

                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        private void ProcessMainToken(byte token)
        {
            if (token >= 16)
            {
                ProcessMatchToken(token);
                return;
            }

            uint count = token;
            if (count == 0)
            {
                byte extension;
                while ((extension = ReadRequiredByte()) == 0)
                {
                    count += 255;
                }
                count += (uint)(15 + extension);
            }
            SetLiterals(count + 3, ParserState.FirstLiteralRun);
        }

        private void ProcessMatchToken(byte token)
        {
            uint length;
            long offset;

            if (token >= 64)
            {
                offset = 1L + ((token >> 2) & 7) + (ReadRequiredByte() << 3);
                length = (uint)((token >> 5) + 1);
                trailingLiteralCount = (uint)(token & 3);
            }
            else if (token >= 32)
            {
                length = (uint)(token & 31);
                if (length == 0)
                {
                    byte extension;
                    while ((extension = ReadRequiredByte()) == 0)
                    {
                        length += 255;
                    }
                    length += (uint)(31 + extension);
                }

                var low = ReadRequiredByte();
                var high = ReadRequiredByte();
                offset = 1L + (low >> 2) + (high << 6);
                length += 2;
                trailingLiteralCount = (uint)(low & 3);
            }
            else if (token >= 16)
            {
                length = (uint)(token & 7);
                if (length == 0)
                {
                    byte extension;
                    while ((extension = ReadRequiredByte()) == 0)
                    {
                        length += 255;
                    }
                    length += (uint)(7 + extension);
                }

                var low = ReadRequiredByte();
                var high = ReadRequiredByte();
                var encodedOffset = (long)((token & 8) << 11) + (low >> 2) + (high << 6);
                trailingLiteralCount = (uint)(low & 3);
                if (encodedOffset == 0)
                {
                    if (length != 1)
                    {
                        throw new InvalidDataException("The LZO block has an invalid end marker.");
                    }
                    if (position != expectedSize)
                    {
                        throw new InvalidDataException("The LZO block ended before the expected output size was reached.");
                    }
                    finished = true;
                    return;
                }

                offset = encodedOffset + 0x4000;
                length += 2;
            }
            else
            {
                offset = 1L + (token >> 2) + (ReadRequiredByte() << 2);
                length = 2;
                trailingLiteralCount = (uint)(token & 3);
            }

            SetMatch(offset, length);
        }

        private void SetLiterals(uint count, ParserState next)
        {
            EnsureOutputCapacity(count);
            pendingLiterals = true;
            pendingCount = count;
            continuation = next;
        }

        private void SetMatch(long offset, uint count)
        {
            if (offset <= 0 || offset > position || offset > HistorySize)
            {
                throw new InvalidDataException("The LZO block contains an invalid look-behind reference.");
            }
            EnsureOutputCapacity(count);
            pendingLiterals = false;
            pendingCount = count;
            matchPosition = position - offset;
            continuation = ParserState.AfterMatch;
        }

        private void EnsureOutputCapacity(uint count)
        {
            if (count > expectedSize - position)
            {
                throw new InvalidDataException("The LZO block expands beyond the expected output size.");
            }
        }

        private void FinishBlock()
        {
            if (finished) return;
            if (pendingCount != 0)
            {
                throw new InvalidDataException("The LZO block expands beyond the expected output size.");
            }
            PrepareSegment();
            if (!finished || pendingCount != 0)
            {
                throw new InvalidDataException("The LZO block expands beyond the expected output size.");
            }
        }

        private void DrainAndFinish()
        {
            if (finished) return;
            while (ReadDecompressedByte() >= 0)
            {
            }
        }

        private byte ReadRequiredByte()
        {
            var value = input.ReadByte();
            if (value < 0)
            {
                throw new EndOfStreamException("The LZO block is truncated.");
            }
            return (byte)value;
        }

        private void ThrowIfDisposed()
        {
            if (disposed) throw new ObjectDisposedException(nameof(LzoDecompressionStream));
        }
    }
}
