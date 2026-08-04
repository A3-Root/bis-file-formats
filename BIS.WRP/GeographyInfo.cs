using BIS.Core.Math;
using BIS.Core.Streams;

namespace BIS.WRP
{
    /// <summary>Represents geography info.</summary>
    public struct GeographyInfo
    {
        private short info;

        /// <summary>Stores the min water depth value.</summary>
        public byte MinWaterDepth => (byte)(info & 0b11);
        /// <summary>Stores the full value.</summary>
        public bool Full => ((info >> 2) & 0b1) > 0;
        /// <summary>Stores the forest value.</summary>
        public bool Forest => ((info >> 3) & 0b1) > 0;
        /// <summary>Stores the road value.</summary>
        public bool Road => ((info >> 4) & 0b1) > 0;
        /// <summary>Stores the max water depth value.</summary>
        public byte MaxWaterDepth => (byte)((info >> 5) & 0b11);
        /// <summary>Stores the how many objects value.</summary>
        public byte HowManyObjects => (byte)((info >> 7) & 0b11);
        /// <summary>Stores the how many hard objects value.</summary>
        public byte HowManyHardObjects => (byte)((info >> 9) & 0b11);
        /// <summary>Stores the gradient value.</summary>
        public byte Gradient => (byte)((info >> 11) & 0b111);
        /// <summary>Stores the some roadway value.</summary>
        public bool SomeRoadway => ((info >> 14) & 0b1) > 0;
        /// <summary>Stores the some objects value.</summary>
        public bool SomeObjects => ((info >> 15) & 0b1) > 0;


        /// <summary>Performs the implicit operator short operation.</summary>
        /// <param name="d">The d value.</param>
        /// <returns>The resulting value.</returns>
        public static implicit operator short(GeographyInfo d)
        {
            return d.info;
        }

        /// <summary>Performs the implicit operator geography info operation.</summary>
        /// <param name="d">The d value.</param>
        /// <returns>The resulting value.</returns>
        public static implicit operator GeographyInfo(short d)
        {
            var g = new GeographyInfo
            {
                info = d
            };
            return g;
        }
    }
}
