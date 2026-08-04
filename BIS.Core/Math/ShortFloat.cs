namespace BIS.Core.Math
{
    /// <summary>Represents short float.</summary>
    public struct ShortFloat
    {
        private const int MANTISSA_SIZE = 10;
        private const int SIGN_BIT = 0x8000;
        private const int M_MASK = 1 << MANTISSA_SIZE;
        private const int EXPONENT_SIZE = ((16 - 1 - 1) - MANTISSA_SIZE);
        private const int E_MASK = (1 << EXPONENT_SIZE);

        private ushort value;

        /// <summary>Initializes a new ShortFloat instance.</summary>
        /// <param name="v">The v value.</param>
        public ShortFloat(ushort v)
        {
            value = v;
        }

        /// <summary>Performs the implicit operator float operation.</summary>
        /// <param name="d">The d value.</param>
        /// <returns>The resulting value.</returns>
        public static implicit operator float(ShortFloat d)
        {
            return (float)d.DoubleValue;
        }


        /// <summary>Gets the double value.</summary>
        public double DoubleValue
        {
            get
            {
                double sign = ((value & SIGN_BIT) != 0) ? -1 : 1;
                int exponent = (value & (SIGN_BIT - 1)) >> MANTISSA_SIZE;
                double significandbits = ((double)(value & (M_MASK - 1)) / M_MASK);

                if (exponent == 0) return ((sign / 0x4000) * (0 + significandbits));
                return (sign * System.Math.Pow(2, exponent - (E_MASK - 1)) * (1 + significandbits));
            }
        }
    }
}
