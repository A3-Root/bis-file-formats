using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace BIS.Core.Streams
{
    /// <summary>Extends <see cref="BinaryWriter"/> with writers for BI file-format primitives and compressed arrays.</summary>
    public class BinaryWriterEx : BinaryWriter
    {
        /// <summary>Gets or sets whether LZO blocks include an explicit compression flag.</summary>
        public bool UseCompressionFlag { get; set; }
        /// <summary>Gets or sets whether compressed blocks use LZO instead of LZSS.</summary>
        public bool UseLZOCompression { get; set; }

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
        /// <summary>Initializes a writer that closes its destination when disposed.</summary>
        /// <param name="dstStream">The destination stream.</param>
        public BinaryWriterEx(Stream dstStream) : base(dstStream, Encoding.ASCII) { }

        /// <summary>Initializes a writer with configurable ownership of its destination.</summary>
        /// <param name="dstStream">The destination stream.</param>
        /// <param name="leaveOpen"><see langword="true"/> to leave the destination open when disposed.</param>
        public BinaryWriterEx(Stream dstStream, bool leaveOpen): base(dstStream, Encoding.ASCII, leaveOpen) {}

        /// <summary>Writes an ASCII string padded with null bytes to a fixed length.</summary>
        /// <param name="text">The text value.</param>
        /// <param name="len">The len value.</param>
        public void WriteAscii(string text, uint len)
        {
            Write(text.ToCharArray());
            uint num = (uint)(len - text.Length);
            for (int index = 0; index < num; ++index)
                Write(char.MinValue); //ToDo: check encoding, should always write one byte and never two or more
        }

        /// <summary>Writes an ASCII string prefixed with its 32-bit length.</summary>
        /// <param name="text">The text value.</param>
        public void WriteAscii32(string text)
        {
            Write(text.Length);
            Write(text.ToCharArray());
        }

        /// <summary>Writes a null-terminated ASCII string.</summary>
        /// <param name="text">The text value.</param>
        public void WriteAsciiz(string text)
        {
            Write(text.ToCharArray());
            Write(char.MinValue);
        }


        /// <summary>Writes a length-prefixed array.</summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="array">The elements to write.</param>
        /// <param name="write">The function that writes one element.</param>
        public void WriteArray<T>(T[] array, Action<BinaryWriterEx, T> write)
        {
            Write(array.Length);
            WriteArrayBase(array, write);
        }

        private void WriteArrayBase<T>(T[] array, Action<BinaryWriterEx, T> write)
        {
            foreach (var item in array)
            {
                write(this, item);
            }
        }

        /// <summary>Writes a length-prefixed compressed array of single-precision values.</summary>
        /// <param name="array">The array to process.</param>
        public void WriteCompressedFloatArray(float[] array)
        {
            WriteCompressedArray(array, (w, v) => w.Write(v), 4);
        }

        /// <summary>Writes a length-prefixed compressed array.</summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="array">The elements to write.</param>
        /// <param name="write">The function that writes one element.</param>
        /// <param name="size">The encoded size of one element, in bytes.</param>
        /// <param name="forceCompressed">Whether to compress a block below the normal LZO threshold.</param>
        public void WriteCompressedArray<T>(T[] array, Action<BinaryWriterEx, T> write, int size, bool forceCompressed = false)
        {
            var mem = new MemoryStream();
            using (var writer = new BinaryWriterEx(mem))
            {
                foreach (var item in array)
                {
                    write(writer, item);
                }
            }
            Write(array.Length);
            var bytes = mem.ToArray();
            if (array.Length * size != bytes.Length)
            {
                throw new InvalidOperationException();
            }
            WriteCompressed(bytes, forceCompressed);
        }

        private void WriteCompressed(byte[] bytes, bool forceCompressed = false)
        {
            if (UseLZOCompression)
            {
                WriteLZO(bytes, forceCompressed);
            }
            else
            {
                WriteLZSS(bytes);
            }
        }


        /// <summary>Writes a raw or LZO-compressed block according to its size and configured flags.</summary>
        /// <param name="bytes">The uncompressed bytes.</param>
        /// <param name="forceCompressed">Whether to compress a block below 1024 bytes.</param>
        public void WriteLZO(byte[] bytes, bool forceCompressed = false)
        {
            if (bytes.Length < 1024 && !forceCompressed)
            {
                if (UseCompressionFlag)
                {
                    Write(false);
                }
                Write(bytes);
            }
            else
            {
                if (UseCompressionFlag)
                {
                    Write(true);
                }
                Write(MiniLZO.MiniLZO.Compress(bytes));
            }
        }

        /// <summary>Writes a raw or LZSS-compressed block followed by its checksum.</summary>
        /// <param name="bytes">The uncompressed bytes.</param>
        /// <param name="inPAA">Whether to always compress and calculate a signed-byte PAA checksum.</param>
        public void WriteLZSS(byte[] bytes, bool inPAA = false)
        {
            if (bytes.Length < 1024 && !inPAA) //data is always compressed in PAAs
            {
                Write(bytes);
            }
            else
            {
                var csum = inPAA ? bytes.Sum(e => (int)(sbyte)e) : bytes.Sum(e => (int)(byte)e);
                using (var lzss = new LzssStream(BaseStream, System.IO.Compression.CompressionMode.Compress, true))
                {
                    lzss.Write(bytes, 0, bytes.Length);
                }
                Write(BitConverter.GetBytes(csum));
            }
            
        }

        /// <summary>Writes the low 24 bits of an unsigned integer in little-endian order.</summary>
        /// <param name="length">The length value.</param>
        public void WriteUInt24(uint length)
        {
            Write((byte)(length & 0xFF));
            Write((byte)((length >> 8) & 0xFF));
            Write((byte)((length >> 16) & 0xFF));
        }

        /// <summary>Writes unprefixed single-precision values.</summary>
        /// <param name="elements">The elements value.</param>
        public void WriteFloats(float[] elements)
        {
            WriteArrayBase(elements, (r, e) => r.Write(e));
        }

        /// <summary>Writes unprefixed unsigned 16-bit integers.</summary>
        /// <param name="elements">The elements value.</param>
        public void WriteUshorts(ushort[] elements)
        {
            WriteArrayBase(elements, (r,e) => r.Write(e));
        }
    }
}
