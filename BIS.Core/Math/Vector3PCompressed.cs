using BIS.Core.Streams;

namespace BIS.Core.Math
{

    /// <summary>Represents vector3 p compressed.</summary>
    public class Vector3PCompressed
    {
        private int value;
        private const float scaleFactor = -1.0f / 511.0f;

        /// <summary>Gets the x.</summary>
        public float X
        {
            get
            {
                int x = value & 0x3FF;
                if (x > 511) x -= 1024;
                return x * scaleFactor;
            }
        }

        /// <summary>Gets the y.</summary>
        public float Y
        {
            get
            {
                int y = (value >> 10) & 0x3FF;
                if (y > 511) y -= 1024;
                return y * scaleFactor;
            }
        }

        /// <summary>Gets the z.</summary>
        public float Z
        {
            get
            {
                int z = (value >> 20) & 0x3FF;
                if (z > 511) z -= 1024;
                return z * scaleFactor;
            }
        }

        /// <summary>Performs the implicit operator vector3 p operation.</summary>
        /// <param name="src">The src value.</param>
        /// <returns>The resulting value.</returns>
        public static implicit operator Vector3P(Vector3PCompressed src)
        {
            int x = src.value & 0x3FF;
            int y = (src.value >> 10) & 0x3FF;
            int z = (src.value >> 20) & 0x3FF;
            if (x > 511) x -= 1024;
            if (y > 511) y -= 1024;
            if (z > 511) z -= 1024;

            return new Vector3P(x * scaleFactor, y * scaleFactor, z * scaleFactor);
        }

        /// <summary>Performs the implicit operator int operation.</summary>
        /// <param name="src">The src value.</param>
        /// <returns>The resulting value.</returns>
        public static implicit operator int(Vector3PCompressed src)
        {
            return src.value;
        }

        /// <summary>Performs the implicit operator vector3 p compressed operation.</summary>
        /// <param name="src">The src value.</param>
        /// <returns>The resulting value.</returns>
        public static implicit operator Vector3PCompressed(int src)
        {
            return new Vector3PCompressed(src);
        }

        /// <summary>Initializes a new Vector3PCompressed instance.</summary>
        /// <param name="value">The value to process.</param>
        public Vector3PCompressed(int value)
        {
            this.value = value;
        }
        /// <summary>Initializes a new Vector3PCompressed instance.</summary>
        /// <param name="input">The source stream or value.</param>
        public Vector3PCompressed(BinaryReaderEx input)
        {
            value = input.ReadInt32();
        }
    }
}
