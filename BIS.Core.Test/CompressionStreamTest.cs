using System;
using System.IO;
using System.Linq;
using BIS.Core.Compression;
using BIS.Core.Streams;
using MiniLzoCodec = MiniLZO.MiniLZO;
using Xunit;

namespace BIS.Core.Test
{
    public class CompressionStreamTest
    {
        [Fact]
        public void LzoStreamSupportsPartialReadsAndLeavesInputAtNextField()
        {
            var expected = CreateData(4097);
            var compressed = MiniLzoCodec.Compress(expected);
            using var input = WithSentinel(compressed, 0x5a);
            using (var stream = new LzoDecompressionStream(input, (uint)expected.Length, true))
            {
                Assert.Equal(expected, ReadInChunks(stream, 37));
                Assert.Equal(-1, stream.ReadByte());
            }
            Assert.Equal(0x5a, input.ReadByte());
        }

        [Fact]
        public void LzoStreamDrainsOnDispose()
        {
            var expected = CreateData(2048);
            using var input = WithSentinel(MiniLzoCodec.Compress(expected), 0x36);
            using (var stream = new LzoDecompressionStream(input, (uint)expected.Length, true))
            {
                var prefix = new byte[19];
                Assert.Equal(prefix.Length, stream.Read(prefix, 0, prefix.Length));
                Assert.Equal(expected.Take(prefix.Length), prefix);
            }
            Assert.Equal(0x36, input.ReadByte());
        }

        [Fact]
        public void LzoStreamWorksWithNonSeekableInput()
        {
            var expected = CreateData(1536);
            using var memory = new MemoryStream(MiniLzoCodec.Compress(expected));
            using var input = new NonSeekableReadStream(memory);
            using var stream = new LzoDecompressionStream(input, (uint)expected.Length, true);
            Assert.Equal(expected, ReadInChunks(stream, 1));
        }

        [Fact]
        public void LzssStreamSupportsPartialReadsAndLeavesInputAtNextField()
        {
            var expected = CreateData(3073);
            using var input = WriteLzssBlock(expected, false, 0x6b);
            using (var stream = new LzssDecompressionStream(input, (uint)expected.Length, false, true))
            {
                Assert.Equal(expected, ReadInChunks(stream, 29));
                Assert.Equal(-1, stream.ReadByte());
            }
            Assert.Equal(0x6b, input.ReadByte());
        }

        [Fact]
        public void LzssStreamSupportsSignedPaaChecksums()
        {
            var expected = Enumerable.Range(0, 1027).Select(i => (byte)(128 + i)).ToArray();
            using var input = WriteLzssBlock(expected, true, 0x22);
            using (var stream = new LzssDecompressionStream(input, (uint)expected.Length, true, true))
            {
                Assert.Equal(expected, ReadInChunks(stream, 7));
            }
            Assert.Equal(0x22, input.ReadByte());
        }

        [Fact]
        public void LzssStreamDrainsOnDispose()
        {
            var expected = CreateData(1300);
            using var input = WriteLzssBlock(expected, false, 0x51);
            using (var stream = new LzssDecompressionStream(input, (uint)expected.Length, false, true))
            {
                Assert.Equal(expected[0], stream.ReadByte());
            }
            Assert.Equal(0x51, input.ReadByte());
        }

        [Fact]
        public void LzssStreamRejectsInvalidChecksum()
        {
            var expected = CreateData(1200);
            using var valid = WriteLzssBlock(expected, false);
            var bytes = valid.ToArray();
            bytes[bytes.Length - 1] ^= 0x40;
            using var input = new MemoryStream(bytes);
            using var stream = new LzssDecompressionStream(input, (uint)expected.Length, false, true);
            Assert.Throws<InvalidDataException>(() => ReadInChunks(stream, 64));
        }

        [Fact]
        public void DecompressionStreamsRejectTruncatedBlocks()
        {
            var expected = CreateData(1600);
            var lzo = MiniLzoCodec.Compress(expected);
            using (var input = new MemoryStream(lzo.Take(lzo.Length - 1).ToArray()))
            using (var stream = new LzoDecompressionStream(input, (uint)expected.Length, true))
            {
                Assert.Throws<EndOfStreamException>(() => ReadInChunks(stream, 23));
            }

            using var validLzss = WriteLzssBlock(expected, false);
            var lzss = validLzss.ToArray();
            using var truncatedInput = new MemoryStream(lzss.Take(lzss.Length - 3).ToArray());
            using var truncatedStream = new LzssDecompressionStream(truncatedInput, (uint)expected.Length, false, true);
            Assert.Throws<EndOfStreamException>(() => ReadInChunks(truncatedStream, 23));
        }

        [Fact]
        public void ReaderUsesRawLzoBelowThresholdAndCompressedLzoAtThreshold()
        {
            var raw = CreateData(1023);
            using (var input = WithSentinel(raw, 0x41))
            using (var reader = new BinaryReaderEx(input) { UseLZOCompression = true })
            {
                Assert.Equal(raw, reader.ReadCompressed((uint)raw.Length));
                Assert.Equal(0x41, reader.ReadByte());
            }

            var compressedData = CreateData(1024);
            using var compressedInput = WithSentinel(MiniLzoCodec.Compress(compressedData), 0x42);
            using var compressedReader = new BinaryReaderEx(compressedInput) { UseLZOCompression = true };
            Assert.Equal(compressedData, compressedReader.ReadCompressed((uint)compressedData.Length));
            Assert.Equal(0x42, compressedReader.ReadByte());
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ReaderHonorsExplicitLzoCompressionFlag(bool compressed)
        {
            var expected = CreateData(64);
            var payload = compressed ? MiniLzoCodec.Compress(expected) : expected;
            using var input = new MemoryStream(new[] { compressed ? (byte)1 : (byte)0 }.Concat(payload).ToArray());
            using var reader = new BinaryReaderEx(input)
            {
                UseLZOCompression = true,
                UseCompressionFlag = true
            };
            Assert.Equal(expected, reader.ReadCompressed((uint)expected.Length));
        }

        [Theory]
        [InlineData(64)]
        [InlineData(1200)]
        public void WriterAndReaderRoundTripExplicitLzoCompressionFlags(int size)
        {
            var expected = CreateData(size);
            using var encoded = new MemoryStream();
            using (var writer = new BinaryWriterEx(encoded, true)
            {
                UseLZOCompression = true,
                UseCompressionFlag = true
            })
            {
                writer.WriteLZO(expected);
            }

            encoded.Position = 0;
            using var reader = new BinaryReaderEx(encoded)
            {
                UseLZOCompression = true,
                UseCompressionFlag = true
            };
            Assert.Equal(expected, reader.ReadCompressed((uint)expected.Length));
        }

        [Fact]
        public void LegacyAndStreamingReaderApisProduceIdenticalData()
        {
            var expected = CreateData(2049);
            var compressed = MiniLzoCodec.Compress(expected);
            using var eagerInput = new MemoryStream(compressed);
            using var eagerReader = new BinaryReaderEx(eagerInput) { UseLZOCompression = true };
            using var streamingInput = new MemoryStream(compressed);
            using var streamingReader = new BinaryReaderEx(streamingInput) { UseLZOCompression = true };

            var eager = eagerReader.ReadCompressed((uint)expected.Length);
            using var stream = streamingReader.ReadCompressedStream((uint)expected.Length);
            Assert.Equal(eager, ReadInChunks(stream, 13));
        }

        [Fact]
        public void EmptyCompressedBlockDoesNotConsumeACompressionFlag()
        {
            using var input = new MemoryStream(new byte[] { 0x73 });
            using var reader = new BinaryReaderEx(input)
            {
                UseLZOCompression = true,
                UseCompressionFlag = true
            };
            Assert.Empty(reader.ReadCompressed(0));
            Assert.Equal(0x73, reader.ReadByte());
        }

        [Fact]
        public void GenericCompressedReaderReadsDirectlyFromDecompressionStream()
        {
            var values = Enumerable.Range(0, 300).Select(i => i * 1.25f).ToArray();
            using var encoded = new MemoryStream();
            using (var writer = new BinaryWriterEx(encoded, true) { UseLZOCompression = true })
            {
                writer.WriteCompressedFloatArray(values);
            }
            encoded.Position = 0;
            using var reader = new BinaryReaderEx(encoded) { UseLZOCompression = true };
            Assert.Equal(values, reader.ReadCompressedFloatArray());
        }

        private static byte[] CreateData(int length) =>
            Enumerable.Range(0, length).Select(i => (byte)((i * 31 + i / 7) & 0xff)).ToArray();

        private static MemoryStream WithSentinel(byte[] data, byte sentinel) =>
            new MemoryStream(data.Concat(new[] { sentinel }).ToArray());

        private static MemoryStream WriteLzssBlock(byte[] data, bool signedChecksum, byte? sentinel = null)
        {
            var result = new MemoryStream();
            using (var writer = new BinaryWriterEx(result, true))
            {
                writer.WriteLZSS(data, signedChecksum);
                if (sentinel.HasValue) writer.Write(sentinel.Value);
            }
            result.Position = 0;
            return result;
        }

        private static byte[] ReadInChunks(Stream stream, int chunkSize)
        {
            using var result = new MemoryStream();
            var buffer = new byte[chunkSize];
            int read;
            while ((read = stream.Read(buffer, 0, buffer.Length)) != 0)
            {
                result.Write(buffer, 0, read);
            }
            return result.ToArray();
        }

        private sealed class NonSeekableReadStream : Stream
        {
            private readonly Stream inner;
            internal NonSeekableReadStream(Stream inner) => this.inner = inner;
            public override bool CanRead => true;
            public override bool CanSeek => false;
            public override bool CanWrite => false;
            public override long Length => throw new NotSupportedException();
            public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
            public override int Read(byte[] buffer, int offset, int count) => inner.Read(buffer, offset, count);
            public override int ReadByte() => inner.ReadByte();
            public override void Flush() { }
            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
            public override void SetLength(long value) => throw new NotSupportedException();
            public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        }
    }
}
