using System;
using System.Collections.Generic;
using BIS.Core.Streams;

namespace BIS.WRP
{
    /// <summary>
    /// Abstraction of a wrp file, binarised or editable
    /// </summary>
    public class AnyWrp : IReadObject, IWrp
    {
        private OPRW binarized;
        private EditableWrp editable;
        private IWrp wrp;

        /// <summary>Stores the land range x value.</summary>
        public int LandRangeX => wrp.LandRangeX;

        /// <summary>Stores the land range y value.</summary>
        public int LandRangeY => wrp.LandRangeY;

        /// <summary>Stores the terrain range x value.</summary>
        public int TerrainRangeX => wrp.TerrainRangeX;

        /// <summary>Stores the terrain range y value.</summary>
        public int TerrainRangeY => wrp.TerrainRangeY;

        /// <summary>Stores the cell size value.</summary>
        public float CellSize => wrp.CellSize;

        /// <summary>Stores the elevation value.</summary>
        public float[] Elevation => wrp.Elevation;

        /// <summary>Stores the mat names value.</summary>
        public string[] MatNames => wrp.MatNames;

        /// <summary>Stores the material index value.</summary>
        public IReadOnlyList<ushort> MaterialIndex => wrp.MaterialIndex;

        /// <summary>Performs the read operation.</summary>
        /// <param name="input">The source stream or value.</param>
        public void Read(BinaryReaderEx input)
        {
            var signature = input.ReadAscii(4);
            switch (signature)
            {
                case "OPRW":
                    binarized = new OPRW();
                    binarized.ReadContent(input);
                    wrp = binarized;
                    editable = null;
                    break;
                case "8WVR":
                    editable = new EditableWrp();
                    editable.ReadContent(input);
                    wrp = editable;
                    binarized = null;
                    break;
                default:
                    throw new InvalidOperationException($"Unknown WRP format '{signature}'");
            }
        }

        /// <summary>Gets editable wrp.</summary>
        /// <returns>The resulting value.</returns>
        public EditableWrp GetEditableWrp()
        {
            if (editable == null)
            {
                if (binarized != null)
                {
                    editable = binarized.ToEditableWrp();
                }
            }
            return editable;
        }

    }
}
