using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BIS.ALB
{
    /// <summary>Represents map area.</summary>
    public class MapArea
    {
        /// <summary>Stores the x1 value.</summary>
        public double X1;
        /// <summary>Stores the y1 value.</summary>
        public double Y1;
        /// <summary>Stores the x2 value.</summary>
        public double X2;
        /// <summary>Stores the y2 value.</summary>
        public double Y2;

        /// <summary>Stores the width value.</summary>
        public double Width => X2 - X1;
        /// <summary>Stores the height value.</summary>
        public double Height => Y2 - Y1;

        /// <summary>Initializes a new MapArea instance.</summary>
        /// <param name="input">The source stream or value.</param>
        /// <param name="readDouble">The read double value.</param>
        public MapArea(BinaryReader input, bool readDouble = true)
        {
            if (readDouble)
            {
                X1 = input.ReadDouble();
                Y1 = input.ReadDouble();
                X2 = input.ReadDouble();
                Y2 = input.ReadDouble();
            }
            else
            {
                X1 = input.ReadSingle();
                Y1 = input.ReadSingle();
                X2 = input.ReadSingle();
                Y2 = input.ReadSingle();
            }
        }

        /// <summary>Converts this value to string.</summary>
        /// <returns>The resulting value.</returns>
        public override string ToString()
        {
            return $"{X1:0.###};{Y1:0.###};{X2:0.###};{Y2:0.###}";
        }
    }
}
