using BIS.Core.Math;
using BIS.Core.Streams;
using System;
using System.IO;

namespace BIS.P3D.MLOD
{
    /// <summary>Specifies point flags values.</summary>
    [Flags]
    public enum PointFlags
    {
        /// <summary>Specifies the none value.</summary>
        NONE = 0,

        /// <summary>Specifies the onland value.</summary>
        ONLAND = 0x1,
        /// <summary>Specifies the underland value.</summary>
        UNDERLAND = 0x2,
        /// <summary>Specifies the aboveland value.</summary>
        ABOVELAND = 0x4,
        /// <summary>Specifies the keepland value.</summary>
        KEEPLAND = 0x8,
        /// <summary>Specifies the land mask value.</summary>
        LAND_MASK = 0xf,

        /// <summary>Specifies the decal value.</summary>
        DECAL = 0x100,
        /// <summary>Specifies the vdecal value.</summary>
        VDECAL = 0x200,
        /// <summary>Specifies the decal mask value.</summary>
        DECAL_MASK = 0x300,

        /// <summary>Specifies the nolight value.</summary>
        NOLIGHT = 0x10,
        /// <summary>Specifies the ambient value.</summary>
        AMBIENT = 0x20,
        /// <summary>Specifies the fulllight value.</summary>
        FULLLIGHT = 0x40,
        /// <summary>Specifies the halflight value.</summary>
        HALFLIGHT = 0x80,
        /// <summary>Specifies the light mask value.</summary>
        LIGHT_MASK = 0xf0,

        /// <summary>Specifies the nofog value.</summary>
        NOFOG = 0x1000,
        /// <summary>Specifies the skyfog value.</summary>
        SKYFOG = 0x2000,
        /// <summary>Specifies the fog mask value.</summary>
        FOG_MASK = 0x3000,

        /// <summary>Specifies the user mask value.</summary>
        USER_MASK = 0xff0000,
        /// <summary>Specifies the user step value.</summary>
        USER_STEP = 0x010000,

        /// <summary>Specifies the special mask value.</summary>
        SPECIAL_MASK = 0xf000000,
        /// <summary>Specifies the special hidden value.</summary>
        SPECIAL_HIDDEN = 0x1000000,

        /// <summary>Specifies the all flags value.</summary>
        ALL_FLAGS = LAND_MASK | DECAL_MASK | LIGHT_MASK | FOG_MASK | USER_MASK | SPECIAL_MASK
    }

    /// <summary>Represents point.</summary>
    public class Point : Vector3P
    {
        /// <summary>Gets the point flags.</summary>
        public PointFlags PointFlags { get; private set; }

        /// <summary>Initializes a new Point instance.</summary>
        /// <param name="pos">The pos value.</param>
        /// <param name="flags">The flags value.</param>
        public Point(Vector3P pos, PointFlags flags) : base(pos.X, pos.Y, pos.Z)
        {
            PointFlags = flags;
        }

        /// <summary>Initializes a new Point instance.</summary>
        /// <param name="input">The source stream or value.</param>
        public Point(BinaryReaderEx input) : base(input)
        {
            PointFlags = (PointFlags)input.ReadInt32();
        }

        /// <summary>Performs the write operation.</summary>
        /// <param name="output">The destination stream or writer.</param>
        public new void Write(BinaryWriterEx output)
        {
            base.Write(output);
            output.Write((int)PointFlags);
        }
    }

    /// <summary>Represents vertex.</summary>
    public class Vertex
    {
        /// <summary>Gets the point index.</summary>
        public int PointIndex { get; private set; }
        /// <summary>Gets the normal index.</summary>
        public int NormalIndex { get; private set; }
        /// <summary>Gets the u.</summary>
        public float U { get; private set; }
        /// <summary>Gets the v.</summary>
        public float V { get; private set; }

        /// <summary>Initializes a new Vertex instance.</summary>
        /// <param name="input">The source stream or value.</param>
        public Vertex(BinaryReaderEx input)
        {
            Read(input);
        }

        /// <summary>Initializes a new Vertex instance.</summary>
        /// <param name="point">The point value.</param>
        /// <param name="normal">The normal value.</param>
        /// <param name="u">The u value.</param>
        /// <param name="v">The v value.</param>
        public Vertex(int point, int normal, float u, float v)
        {
            PointIndex = point;
            NormalIndex = normal;
            U = u;
            V = v;
        }

        /// <summary>Performs the read operation.</summary>
        /// <param name="input">The source stream or value.</param>
        public void Read(BinaryReaderEx input)
        {
            PointIndex = input.ReadInt32();
            NormalIndex = input.ReadInt32();
            U = input.ReadSingle();
            V = input.ReadSingle();
        }

        /// <summary>Performs the write operation.</summary>
        /// <param name="output">The destination stream or writer.</param>
        public void Write(BinaryWriter output)
        {
            output.Write(PointIndex);
            output.Write(NormalIndex);
            output.Write(U);
            output.Write(V);
        }
    }
}
