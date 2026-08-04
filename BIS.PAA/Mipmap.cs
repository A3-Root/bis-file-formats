using BIS.Core.Streams;
using System;

namespace BIS.PAA
{
    /// <summary>Represents mipmap.</summary>
    public class Mipmap
    {
        private const ushort MAGIC_LZW_W = 1234;
        private const ushort MAGIC_LZW_H = 8765;

        private bool hasMagicLZW;

        /// <summary>Gets the offset.</summary>
        public int Offset { get; }
        /// <summary>Gets the data offset.</summary>
        public int DataOffset { get; }
        /// <summary>Gets the is lzo compressed.</summary>
        public bool IsLZOCompressed { get; }
        /// <summary>Gets the width.</summary>
        public ushort Width { get; }
        /// <summary>Gets the height.</summary>
        public ushort Height { get; }
        /// <summary>Gets the data size.</summary>
        public uint DataSize { get; }

        /// <summary>Initializes a new Mipmap instance.</summary>
        /// <param name="input">The source stream or value.</param>
        /// <param name="offset">The offset value.</param>
        public Mipmap(BinaryReaderEx input, int offset)
        {
            Offset = offset;

            IsLZOCompressed = false;
            hasMagicLZW = false;
            Width = input.ReadUInt16();
            Height = input.ReadUInt16();

            if (Width == MAGIC_LZW_W && Height == MAGIC_LZW_H)
            {
                hasMagicLZW = true;
                Width = input.ReadUInt16();
                Height = input.ReadUInt16();
            }

            if ((Width & 0x8000) != 0)
            {
                Width &= 0x7FFF;
                IsLZOCompressed = true;
            }

            DataSize = input.ReadUInt24();
            DataOffset = (int)input.Position;
            input.Position += DataSize; //skip data
        }

        /// <summary>Gets raw pixel data.</summary>
        /// <param name="input">The source stream or value.</param>
        /// <param name="type">The type value.</param>
        /// <returns>The resulting values.</returns>
        public byte[] GetRawPixelData(BinaryReaderEx input, PAAType type)
        {
            input.Position = DataOffset;

            uint expectedSize = (uint)(Width * Height);

            switch (type)
            {
                case PAAType.AI88:
                case PAAType.RGBA_5551:
                case PAAType.RGBA_4444:
                    expectedSize *= 2;
                    return input.ReadLZSS(expectedSize, true);

                case PAAType.P8:
                    return !hasMagicLZW ?
                        input.ReadCompressedIndices((int)DataSize, expectedSize) :
                        input.ReadLZSS(expectedSize, true);

                case PAAType.RGBA_8888:
                    expectedSize *= 4;
                    return input.ReadLZSS(expectedSize, true);

                case PAAType.DXT1:
                    expectedSize /= 2;
                    goto case PAAType.DXT2;
                case PAAType.DXT2:
                case PAAType.DXT3:
                case PAAType.DXT4:
                case PAAType.DXT5:
                    return !IsLZOCompressed ?
                        input.ReadBytes((int)DataSize) :
                        input.ReadLZO(expectedSize);

                default: throw new ArgumentException("Unexpected PAA type", nameof(type));
            }
        }
    }
}
