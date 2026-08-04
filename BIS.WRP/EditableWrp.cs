using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BIS.Core.Streams;

namespace BIS.WRP
{
    /// <summary>Represents editable wrp.</summary>
    public class EditableWrp : IReadWriteObject, IWrp// aka 8WVR
    {
        /// <summary>Initializes a new EditableWrp instance.</summary>
        public EditableWrp()
        {

        }

        /// <summary>Initializes a new EditableWrp instance.</summary>
        /// <param name="s">The s value.</param>
        public EditableWrp(Stream s)
            : this(new BinaryReaderEx(s))
        {
        }

        /// <summary>Initializes a new EditableWrp instance.</summary>
        /// <param name="input">The source stream or value.</param>
        public EditableWrp(BinaryReaderEx input)
        {
            Read(input);
        }

        /// <summary>Performs the read operation.</summary>
        /// <param name="input">The source stream or value.</param>
        public void Read(BinaryReaderEx input)
        {
            if (input.ReadAscii(4) != "8WVR")
            {
                throw new FormatException("8WVR file does not start with correct file signature");
            }

            ReadContent(input);
        }

        internal void ReadContent(BinaryReaderEx input)
        {
            LandRangeX = input.ReadInt32();
            LandRangeY = input.ReadInt32();
            TerrainRangeX = input.ReadInt32();
            TerrainRangeY = input.ReadInt32();
            CellSize = input.ReadSingle();
            Elevation = input.ReadFloats(TerrainRangeX * TerrainRangeY);
            MaterialIndex = input.ReadUshorts(LandRangeX * LandRangeY);

            var nMaterials = input.ReadInt32();
            MatNames = new string[nMaterials];
            for (int i = 0; i < nMaterials; i++)
            {
                int len;
                do
                {
                    len = input.ReadInt32();
                    if (len != 0)
                    {
                        MatNames[i] = input.ReadAscii(len);
                    }
                } while (len != 0);
            }

            while (!input.HasReachedEnd)
            {
                Objects.Add(new EditableWrpObject(input));
            }
        }

        /// <summary>Performs the write operation.</summary>
        /// <param name="output">The destination stream or writer.</param>
        public void Write(BinaryWriterEx output)
        {
            output.WriteAscii("8WVR", 4);
            output.Write(LandRangeX);
            output.Write(LandRangeY);
            output.Write(TerrainRangeX );
            output.Write(TerrainRangeY);
            output.Write(CellSize);
            output.WriteFloats(Elevation);
            output.WriteUshorts(MaterialIndex);
            output.Write(MatNames.Length);
            foreach (var mat in MatNames)
            {
                if (!string.IsNullOrEmpty(mat))
                {
                    output.WriteAscii32(mat);
                }
                output.WriteAscii32("");
            }
            foreach(var obj in Objects)
            {
                obj.Write(output);
            }
        }


        /// <summary>Gets or sets the land range x.</summary>
        public int LandRangeX { get; set; }
        /// <summary>Gets or sets the land range y.</summary>
        public int LandRangeY { get; set; }
        /// <summary>Gets or sets the terrain range x.</summary>
        public int TerrainRangeX { get; set; }
        /// <summary>Gets or sets the terrain range y.</summary>
        public int TerrainRangeY { get; set; }
        /// <summary>Gets or sets the cell size.</summary>
        public float CellSize { get; set; }
        /// <summary>Gets or sets the elevation.</summary>
        public float[] Elevation { get; set; }
        /// <summary>Gets or sets the material index.</summary>
        public ushort[] MaterialIndex { get; set; }
        /// <summary>Gets or sets the mat names.</summary>
        public string[] MatNames { get; set; }
        /// <summary>Gets or sets the objects.</summary>
        public List<EditableWrpObject> Objects { get; set; } = new List<EditableWrpObject>();

        IReadOnlyList<ushort> IWrp.MaterialIndex => MaterialIndex;

        /// <summary>Gets non dummy objects.</summary>
        /// <returns>The resulting values.</returns>
        public IEnumerable<EditableWrpObject> GetNonDummyObjects() => Objects.TakeWhile(o => !string.IsNullOrEmpty(o.Model));
    }
}
