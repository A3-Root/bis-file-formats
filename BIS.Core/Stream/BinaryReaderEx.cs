using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using BIS.Core.Compression;

namespace BIS.Core.Streams
{
    /// <summary>Extends <see cref="BinaryReader"/> with readers for BI file-format primitives and compressed arrays.</summary>
    public class BinaryReaderEx : BinaryReader
    {
        /// <summary>Gets or sets whether compressed blocks begin with an explicit compression flag.</summary>
        public bool UseCompressionFlag { get; set; }
        /// <summary>Gets or sets whether compressed blocks use LZO instead of LZSS.</summary>
        public bool UseLZOCompression { get; set; }

        //used to store file format versions (e.g. ODOL v60)
        /// <summary>Gets or sets the format version associated with the input stream.</summary>
        public int Version { get; set; }

        /// <summary>Gets or sets the position of the underlying stream.</summary>
        public long Position
        {
            get
            {
                return BaseStream.Position;
            }
            set
            {
                BaseStream.Position = value;
            }
        }

        /// <summary>Gets whether the underlying stream is positioned at its end.</summary>
        public bool HasReachedEnd => BaseStream.Position == BaseStream.Length;

        /// <summary>Initializes a reader over the specified stream.</summary>
        /// <param name="stream">The stream to read.</param>
        public BinaryReaderEx(Stream stream): base(stream)
        {
            UseCompressionFlag = false;
        }


        /// <summary>Reads a little-endian unsigned 24-bit integer.</summary>
        /// <returns>The decoded integer.</returns>
        public uint ReadUInt24()
        {
            return (uint)(ReadByte() + (ReadByte() << 8) + (ReadByte() << 16));
        }

        /// <summary>Reads a fixed number of ASCII bytes.</summary>
        /// <param name="count">The number of bytes to read.</param>
        /// <returns>The decoded string.</returns>
        public string ReadAscii(int count)
        {
            var str = new StringBuilder();
            for (int index = 0; index < count; ++index)
                str.Append((char)ReadByte());
            return str.ToString();
        }

        /// <summary>Reads an ASCII string prefixed by a 16-bit byte count.</summary>
        /// <returns>The decoded string.</returns>
        public string ReadAscii()
        {
            var n = ReadUInt16();
            return ReadAscii(n);
        }

        /// <summary>Reads an ASCII string prefixed by a 32-bit byte count.</summary>
        /// <returns>The decoded string.</returns>
        public string ReadAscii32()
        {
            var n = ReadUInt32();
            return ReadAscii((int)n);
        }

        /// <summary>Reads a null-terminated ASCII string.</summary>
        /// <returns>The decoded string without its terminator.</returns>
        public string ReadAsciiz()
        {
            var str = new StringBuilder();
            char ch;
            while ((ch = (char)ReadByte()) != 0)
                str.Append(ch);
            return str.ToString();
        }

        #region SimpleArray
        /// <summary>Reads a fixed number of elements with a caller-provided decoder.</summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="readElement">The function that reads one element.</param>
        /// <param name="size">The number of elements.</param>
        /// <returns>The decoded elements.</returns>
        public T[] ReadArrayBase<T>(Func<BinaryReaderEx, T> readElement, int size)
        {
            var array = new T[size];
            for (int i = 0; i < size; i++)
                array[i] = readElement(this);

            return array;
        }

        /// <summary>Reads a 32-bit length-prefixed array.</summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="readElement">The function that reads one element.</param>
        /// <returns>The decoded elements.</returns>
        public T[] ReadArray<T>(Func<BinaryReaderEx, T> readElement) => ReadArrayBase(readElement, ReadInt32());
        /// <summary>Reads a 32-bit length-prefixed array of single-precision values.</summary>
        /// <returns>The resulting values.</returns>
        public float[] ReadFloatArray() => ReadArray(i => i.ReadSingle());
        /// <summary>Reads a 32-bit length-prefixed array of signed integers.</summary>
        /// <returns>The resulting values.</returns>
        public int[] ReadIntArray() => ReadArray(i => i.ReadInt32());
        /// <summary>Reads a 32-bit length-prefixed array of null-terminated ASCII strings.</summary>
        /// <returns>The resulting values.</returns>
        public string[] ReadStringArray() => ReadArray(i => i.ReadAsciiz());

        #endregion

        #region CompressedArray
        /// <summary>Reads a length-prefixed compressed array.</summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="readElement">The function that reads one decompressed element.</param>
        /// <param name="elemSize">The encoded size of one element, in bytes.</param>
        /// <returns>The decoded elements.</returns>
        public T[] ReadCompressedArray<T>(Func<BinaryReaderEx, T> readElement, int elemSize)
        {
            int nElements = ReadInt32();
            return ReadCompressed<T>(readElement, nElements, elemSize);
        }

        /// <summary>Reads a length-prefixed compressed array of 16-bit integers.</summary>
        /// <returns>The resulting values.</returns>
        public short[] ReadCompressedShortArray() => ReadCompressedArray(i => i.ReadInt16(), 2);
        /// <summary>Reads a length-prefixed compressed array of 32-bit integers.</summary>
        /// <returns>The resulting values.</returns>
        public int[] ReadCompressedIntArray() => ReadCompressedArray(i => i.ReadInt32(), 4);        
        /// <summary>Reads a length-prefixed compressed array of single-precision values.</summary>
        /// <returns>The resulting values.</returns>
        public float[] ReadCompressedFloatArray() => ReadCompressedArray(i => i.ReadSingle(), 4);
        /// <summary>Reads a length-prefixed compressed byte array.</summary>
        /// <returns>The resulting values.</returns>
        public byte[] ReadCompressedByteArray() => ReadCompressedArray(i => i.ReadByte(), 1);

        #endregion

        #region CondensedArray

        /// <summary>Reads a condensed array stored as either a repeated value or a compressed block.</summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="readElement">The function that reads one element.</param>
        /// <param name="sizeOfT">The encoded size of one element, in bytes.</param>
        /// <returns>The decoded elements.</returns>
        public T[] ReadCondensedArray<T>(Func<BinaryReaderEx, T> readElement, int sizeOfT)
        {
            int size = ReadInt32();
            T[] result = new T[size];
            bool defaultFill = ReadBoolean();
            if (defaultFill)
            {
                var defaultValue = readElement(this);
                for (int i = 0; i < size; i++)
                    result[i] = defaultValue;

                return result;
            }

            var expectedDataSize = (uint)(size * sizeOfT);
            using (var compressed = ReadCompressedStream(expectedDataSize))
            using (var stream = new BinaryReaderEx(compressed))
            {
                result = stream.ReadArrayBase(readElement, size);
            }

            return result;
        }

        /// <summary>Reads a condensed array of 32-bit integers.</summary>
        /// <returns>The resulting values.</returns>
        public int[] ReadCondensedIntArray() => ReadCondensedArray(i => i.ReadInt32(), 4);
        #endregion

        /// <summary>Reads a variable-length unsigned 7-bit integer into a signed 32-bit value.</summary>
        /// <returns>The resulting value.</returns>
        public int ReadCompactInteger()
        {
            int result = 0;
            int i = 0;
            bool end;
            do
            {
                int b = ReadByte();
                result |= (b & 0x7f) << (i * 7);
                end = b < 0x80;
                i++;
            } while (!end);
            return result;
        }

        /// <summary>Reads and materializes a compressed or raw block.</summary>
        /// <param name="expectedSize">The exact decompressed size.</param>
        /// <param name="forceCompressed">Whether a flagless LZO block is compressed even below the normal threshold.</param>
        /// <returns>The decompressed bytes.</returns>
        public byte[] ReadCompressed(uint expectedSize, bool forceCompressed = false)
        {
            using (var stream = ReadCompressedStream(expectedSize, forceCompressed))
            {
                return ReadAll(stream, expectedSize);
            }
        }

        /// <summary>Reads and materializes an LZO-compressed or raw block.</summary>
        /// <param name="expectedSize">The exact decompressed size.</param>
        /// <param name="isCompressed">Whether to force compression when no explicit flag is present.</param>
        /// <returns>The decompressed bytes.</returns>
        public byte[] ReadLZO(uint expectedSize, bool isCompressed = false)
        {
            using (var stream = ReadLZOStream(expectedSize, isCompressed))
            {
                return ReadAll(stream, expectedSize);
            }
        }

        /// <summary>Reads and materializes an LZSS-compressed or raw block.</summary>
        /// <param name="expectedSize">The exact decompressed size.</param>
        /// <param name="inPAA">Whether the block is always compressed and uses a signed-byte checksum.</param>
        /// <returns>The decompressed bytes.</returns>
        public byte[] ReadLZSS(uint expectedSize, bool inPAA = false)
        {
            using (var stream = ReadLZSSStream(expectedSize, inPAA))
            {
                return ReadAll(stream, expectedSize);
            }
        }

        /// <summary>Creates a forward-only stream over a compressed or raw block.</summary>
        /// <param name="expectedSize">The exact decompressed size.</param>
        /// <param name="forceCompressed">Whether a flagless LZO block is compressed even below the normal threshold.</param>
        /// <returns>A bounded stream that leaves this reader's base stream open.</returns>
        public Stream ReadCompressedStream(uint expectedSize, bool forceCompressed = false)
        {
            if (expectedSize == 0)
            {
                return new BoundedReadStream(BaseStream, 0, true);
            }
            return UseLZOCompression
                ? ReadLZOStream(expectedSize, forceCompressed)
                : ReadLZSSStream(expectedSize);
        }

        /// <summary>Creates a forward-only stream over an LZO-compressed or raw block.</summary>
        /// <param name="expectedSize">The exact decompressed size.</param>
        /// <param name="isCompressed">Whether to force compression when no explicit flag is present.</param>
        /// <returns>A bounded stream that leaves this reader's base stream open.</returns>
        public Stream ReadLZOStream(uint expectedSize, bool isCompressed = false)
        {
            if (expectedSize == 0)
            {
                return new BoundedReadStream(BaseStream, 0, true);
            }
            if (UseCompressionFlag)
            {
                isCompressed = ReadBoolean();
            }
            else
            {
                isCompressed = isCompressed || expectedSize >= 1024;
            }

            if (!isCompressed)
            {
                return new BoundedReadStream(BaseStream, expectedSize, true);
            }

            return new LzoDecompressionStream(BaseStream, expectedSize, true);
        }

        /// <summary>Creates a forward-only stream over an LZSS-compressed or raw block.</summary>
        /// <param name="expectedSize">The exact decompressed size.</param>
        /// <param name="inPAA">Whether the block is always compressed and uses a signed-byte checksum.</param>
        /// <returns>A bounded stream that leaves this reader's base stream open.</returns>
        public Stream ReadLZSSStream(uint expectedSize, bool inPAA = false)
        {
            if (expectedSize < 1024 && !inPAA)
            {
                return new BoundedReadStream(BaseStream, expectedSize, true);
            }
            return new LzssDecompressionStream(BaseStream, expectedSize, inPAA, true);
        }

        /// <summary>Reads run-length encoded byte indices.</summary>
        /// <param name="bytesToRead">The number of encoded runs or literals to consume.</param>
        /// <param name="expectedSize">The expected decoded byte count.</param>
        /// <returns>The decoded indices.</returns>
        public byte[] ReadCompressedIndices(int bytesToRead, uint expectedSize)
        {
            var result = new byte[expectedSize];
            int outputI = 0;
            for(int i=0;i<bytesToRead;i++)
            {
                var b = ReadByte();
                if( (b & 128) != 0 )
                {
                    byte n = (byte)(b - 127);
                    byte value = ReadByte();
                    for (int j = 0; j < n; j++)
                        result[outputI++] = value;
                }
                else
                {
                    for (int j = 0; j < b + 1; j++)
                        result[outputI++] = ReadByte();
                }
            }

            Debug.Assert(outputI == expectedSize);

            return result;
        }

        /// <summary>Reads a fixed number of compressed single-precision values.</summary>
        /// <param name="nElements">The number of values.</param>
        /// <returns>The decoded values.</returns>
        public float[] ReadCompressedFloats(int nElements)
        {
            return ReadCompressed(r => r.ReadSingle(), nElements, 4);
        }

        /// <summary>Reads a fixed number of uncompressed single-precision values.</summary>
        /// <param name="nElements">The number of values.</param>
        /// <returns>The decoded values.</returns>
        public float[] ReadFloats(int nElements)
        {
            return ReadArrayBase(r => r.ReadSingle(), nElements);
        }

        /// <summary>Reads a fixed number of unsigned 16-bit integers.</summary>
        /// <param name="nElements">The number of values.</param>
        /// <returns>The decoded values.</returns>
        public ushort[] ReadUshorts(int nElements)
        {
            return ReadArrayBase(r => r.ReadUInt16(), nElements);
        }

        /// <summary>Reads a fixed number of elements directly from a decompression stream.</summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="readElement">The function that reads one decompressed element.</param>
        /// <param name="nElements">The number of elements.</param>
        /// <param name="elemSize">The encoded size of one element, in bytes.</param>
        /// <returns>The decoded elements.</returns>
        public T[] ReadCompressed<T>(Func<BinaryReaderEx, T> readElement, int nElements, int elemSize)
        {
            var expectedDataSize = (uint)(nElements * elemSize);
            using (var compressed = ReadCompressedStream(expectedDataSize))
            using (var stream = new BinaryReaderEx(compressed))
            {
                return stream.ReadArrayBase(readElement, nElements);
            }
        }

        private static byte[] ReadAll(Stream stream, uint expectedSize)
        {
            var result = new byte[checked((int)expectedSize)];
            var offset = 0;
            while (offset < result.Length)
            {
                var read = stream.Read(result, offset, result.Length - offset);
                if (read == 0)
                {
                    throw new EndOfStreamException("The compressed block ended before the expected output size was reached.");
                }
                offset += read;
            }
            return result;
        }
    }
}
