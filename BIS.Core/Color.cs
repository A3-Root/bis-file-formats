using System.Globalization;
using System.Diagnostics;

using BIS.Core.Streams;

namespace BIS.Core
{
    /// <summary>Represents color p.</summary>
    public struct ColorP
    {
        /// <summary>Gets the red.</summary>
        public float Red { get; private set; }
        /// <summary>Gets the green.</summary>
        public float Green { get; private set; }
        /// <summary>Gets the blue.</summary>
        public float Blue { get; private set; }
        /// <summary>Gets the alpha.</summary>
        public float Alpha { get; private set; }

        /// <summary>Initializes a new ColorP instance.</summary>
        /// <param name="r">The r value.</param>
        /// <param name="g">The g value.</param>
        /// <param name="b">The b value.</param>
        /// <param name="a">The a value.</param>
        public ColorP(float r, float g, float b, float a)
        {
            Red = r;
            Green = g;
            Blue = b;
            Alpha = a;
        }
        /// <summary>Initializes a new ColorP instance.</summary>
        /// <param name="input">The source stream or value.</param>
        public ColorP(BinaryReaderEx input)
        {
            Red = input.ReadSingle();
            Green = input.ReadSingle();
            Blue = input.ReadSingle();
            Alpha = input.ReadSingle();
        }

        /// <summary>Performs the read operation.</summary>
        /// <param name="input">The source stream or value.</param>
        public void Read(BinaryReaderEx input)
        {
            Red = input.ReadSingle();
            Green = input.ReadSingle();
            Blue = input.ReadSingle();
            Alpha = input.ReadSingle();
        }

        /// <summary>Performs the write operation.</summary>
        /// <param name="output">The destination stream or writer.</param>
        public void Write(BinaryWriterEx output)
        {
            output.Write(Red);
            output.Write(Green);
            output.Write(Blue);
            output.Write(Alpha);
        }

        /// <summary>Converts this value to string.</summary>
        /// <returns>The resulting value.</returns>
        public override string ToString()
        {
            CultureInfo cultureInfo = new CultureInfo("en-GB");
            return "{" + Red.ToString(cultureInfo.NumberFormat) + "," + Green.ToString(cultureInfo.NumberFormat) + "," + this.Blue.ToString(cultureInfo.NumberFormat) + "," + this.Alpha.ToString(cultureInfo.NumberFormat) + "}";
        }
    }

    /// <summary>Represents packed color.</summary>
    public struct PackedColor
    {
        private uint value;

        /// <summary>Stores the a8 value.</summary>
        public byte A8 => (byte)((value >> 24) & 0xff);
        /// <summary>Stores the r8 value.</summary>
        public byte R8 => (byte)((value >> 16) & 0xff);
        /// <summary>Stores the g8 value.</summary>
        public byte G8 => (byte)((value >>  8) & 0xff);
        /// <summary>Stores the b8 value.</summary>
        public byte B8 => (byte)((value      ) & 0xff);

        /// <summary>Initializes a new PackedColor instance.</summary>
        /// <param name="value">The value to process.</param>
        public PackedColor(uint value)
        {
            this.value = value;
        }

        /// <summary>Initializes a new PackedColor instance.</summary>
        /// <param name="r">The r value.</param>
        /// <param name="g">The g value.</param>
        /// <param name="b">The b value.</param>
        /// <param name="a">The a value.</param>
        public PackedColor(byte r, byte g, byte b, byte a=255)
        {
            value = PackColor(r, g, b, a);
        }

        /// <summary>Initializes a new PackedColor instance.</summary>
        /// <param name="r">The r value.</param>
        /// <param name="g">The g value.</param>
        /// <param name="b">The b value.</param>
        /// <param name="a">The a value.</param>
        public PackedColor(float r, float g, float b, float a)
        {
            Debug.Assert(r <= 1.0f && r >= 0 && !float.IsNaN(r));
            Debug.Assert(g <= 1.0f && g >= 0 && !float.IsNaN(g));
            Debug.Assert(b <= 1.0f && b >= 0 && !float.IsNaN(b));
            Debug.Assert(a <= 1.0f && a >= 0 && !float.IsNaN(a));

            byte r8 = (byte)(r * 255);
            byte g8 = (byte)(g * 255);
            byte b8 = (byte)(b * 255);
            byte a8 = (byte)(a * 255);

            value = PackColor(r8, g8, b8, a8);
        }

        internal static uint PackColor(byte r, byte g, byte b, byte a)
        {
            return (uint)(a << 24 | r << 16 | g << 8) | b;
        }
    }
}
