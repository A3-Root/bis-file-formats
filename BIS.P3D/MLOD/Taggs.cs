using BIS.Core.Math;
using BIS.Core.Streams;
using System;

namespace BIS.P3D.MLOD
{
    /// <summary>Represents tagg.</summary>
    public abstract class Tagg
    {
        /// <summary>Gets or sets the name.</summary>
        public string Name { get; set; }
        /// <summary>Gets or sets the data size.</summary>
        public uint DataSize { get; set; }

        /// <summary>Initializes a new Tagg instance.</summary>
        /// <param name="dataSize">The data size value.</param>
        /// <param name="taggName">The tagg name value.</param>
        protected Tagg(uint dataSize, string taggName)
        {
            Name = taggName;
            DataSize = dataSize;
        }

        /// <summary>Initializes a new Tagg instance.</summary>
        /// <param name="input">The source stream or value.</param>
        protected Tagg(BinaryReaderEx input)
        {
            if (!input.ReadBoolean())
                throw new FormatException("Deactivated Tagg?");

            Name = input.ReadAsciiz();
            DataSize = input.ReadUInt32();
        }

        /// <summary>Writes header to the underlying data.</summary>
        /// <param name="output">The destination stream or writer.</param>
        protected void WriteHeader(BinaryWriterEx output)
        {
            output.Write(true);
            output.WriteAsciiz(Name);
            output.Write(DataSize);
        }

        /// <summary>Performs the write operation.</summary>
        /// <param name="output">The destination stream or writer.</param>
        public abstract void Write(BinaryWriterEx output);

        /// <summary>Reads tagg from the underlying data.</summary>
        /// <param name="input">The source stream or value.</param>
        /// <param name="nPoints">The n points value.</param>
        /// <param name="faces">The faces value.</param>
        /// <returns>The resulting value.</returns>
        public static Tagg ReadTagg(BinaryReaderEx input, int nPoints, Face[] faces)
        {
            if (!input.ReadBoolean())
                throw new Exception("Deactivated Tagg?");
            var taggName = input.ReadAsciiz();
            input.Position -= taggName.Length + 2;

            switch (taggName)
            {
                case "#SharpEdges#":
                    return new SharpEdgesTagg(input);
                case "#Property#":
                    return new PropertyTagg(input);
                case "#Mass#":
                    return new MassTagg(input);
                case "#UVSet#":
                    return new UVSetTagg(input, faces);
                case "#Lock#":
                    return new LockTagg(input, nPoints, faces.Length);
                case "#Selected#":
                    return new SelectedTagg(input, nPoints, faces.Length);
                case "#Animation#":
                    return new AnimationTagg(input);
                case "#EndOfFile#":
                    return new EOFTagg(input);
                default:
                    return new NamedSelectionTagg(input, nPoints, faces.Length);
            }
        }
    }

    /// <summary>Represents animation tagg.</summary>
    public class AnimationTagg : Tagg
    {
        /// <summary>Gets or sets the frame time.</summary>
        public float FrameTime { get; set; }
        /// <summary>Gets or sets the frame points.</summary>
        public Vector3P[] FramePoints { get; set; }

        /// <summary>Initializes a new AnimationTagg instance.</summary>
        /// <param name="frameTime">The frame time value.</param>
        /// <param name="framePoints">The frame points value.</param>
        public AnimationTagg(float frameTime, Vector3P[] framePoints) : base((uint)(framePoints.Length * 4 + 4), "#Animation#")
        {
            FrameTime = frameTime;
            FramePoints = framePoints;
        }

        /// <summary>Initializes a new AnimationTagg instance.</summary>
        /// <param name="input">The source stream or value.</param>
        public AnimationTagg(BinaryReaderEx input) : base(input)
        {
            Read(input);
        }

        /// <summary>Performs the read operation.</summary>
        /// <param name="input">The source stream or value.</param>
        public void Read(BinaryReaderEx input)
        {
            var num = (DataSize - 4) / 12;
            FrameTime = input.ReadSingle();
            FramePoints = new Vector3P[num];
            for (int i = 0; i < num; ++i)
                FramePoints[i] = new Vector3P(input);
        }

        /// <summary>Performs the write operation.</summary>
        /// <param name="output">The destination stream or writer.</param>
        public override void Write(BinaryWriterEx output)
        {
            WriteHeader(output);
            output.Write(FrameTime);
            for (int index = 0; index < FramePoints.Length; ++index)
                FramePoints[index].Write(output);
        }
    }

    /// <summary>Represents lock tagg.</summary>
    public class LockTagg : Tagg
    {
        /// <summary>Gets the locked points.</summary>
        public bool[] LockedPoints { get; private set; }
        /// <summary>Gets the locked faces.</summary>
        public bool[] LockedFaces { get; private set; }

        /// <summary>Initializes a new LockTagg instance.</summary>
        /// <param name="input">The source stream or value.</param>
        /// <param name="nPoints">The n points value.</param>
        /// <param name="nFaces">The n faces value.</param>
        public LockTagg(BinaryReaderEx input, int nPoints, int nFaces) : base(input)
        {
            Read(input, nPoints, nFaces);
        }

        /// <summary>Performs the read operation.</summary>
        /// <param name="input">The source stream or value.</param>
        /// <param name="nPoints">The n points value.</param>
        /// <param name="nFaces">The n faces value.</param>
        public void Read(BinaryReaderEx input, int nPoints, int nFaces)
        {
            LockedPoints = new bool[nPoints];
            for (int index = 0; index < nPoints; ++index)
                LockedPoints[index] = input.ReadBoolean();
            LockedFaces = new bool[nFaces];
            for (int index = 0; index < nFaces; ++index)
                LockedFaces[index] = input.ReadBoolean();
        }

        /// <summary>Performs the write operation.</summary>
        /// <param name="output">The destination stream or writer.</param>
        public override void Write(BinaryWriterEx output)
        {
            WriteHeader(output);
            for (int index = 0; index < LockedPoints.Length; ++index)
                output.Write(LockedPoints[index]);
            for (int index = 0; index < LockedFaces.Length; ++index)
                output.Write(LockedFaces[index]);
        }
    }

    /// <summary>Represents mass tagg.</summary>
    public class MassTagg : Tagg
    {
        /// <summary>Gets or sets the mass.</summary>
        public float[] Mass { get; set; }

        /// <summary>Initializes a new MassTagg instance.</summary>
        /// <param name="mass">The mass value.</param>
        public MassTagg(float[] mass): base((uint)(mass.Length * 4), "#Mass#")
        {
            Mass = mass;
        }

        /// <summary>Initializes a new MassTagg instance.</summary>
        /// <param name="input">The source stream or value.</param>
        public MassTagg(BinaryReaderEx input): base(input)
        {
            Read(input);
        }

        /// <summary>Performs the read operation.</summary>
        /// <param name="input">The source stream or value.</param>
        public void Read(BinaryReaderEx input)
        {
            uint num = DataSize / 4;
            Mass = new float[num];
            for (int index = 0; index < num; ++index)
                Mass[index] = input.ReadSingle();
        }

        /// <summary>Performs the write operation.</summary>
        /// <param name="output">The destination stream or writer.</param>
        public override void Write(BinaryWriterEx output)
        {
            WriteHeader(output);
            uint num = DataSize / 4;
            for (int index = 0; index < num; ++index)
                output.Write(Mass[index]);
        }
    }

    /// <summary>Represents named selection tagg.</summary>
    public class NamedSelectionTagg : Tagg
    {
        /// <summary>Gets or sets the points.</summary>
        public byte[] Points { get; set; }
        /// <summary>Gets or sets the faces.</summary>
        public byte[] Faces { get; set; }

        /// <summary>Initializes a new NamedSelectionTagg instance.</summary>
        /// <param name="name">The name value.</param>
        /// <param name="points">The points value.</param>
        /// <param name="faces">The faces value.</param>
        public NamedSelectionTagg(string name, byte[] points, byte[] faces) : base((uint)(points.Length + faces.Length), name)
        {
            Points = points;
            Faces = faces;
        }

        /// <summary>Initializes a new NamedSelectionTagg instance.</summary>
        /// <param name="input">The source stream or value.</param>
        /// <param name="nPoints">The n points value.</param>
        /// <param name="nFaces">The n faces value.</param>
        public NamedSelectionTagg(BinaryReaderEx input, int nPoints, int nFaces) : base(input)
        {
            Read(input, nPoints, nFaces);
        }

        /// <summary>Performs the read operation.</summary>
        /// <param name="input">The source stream or value.</param>
        /// <param name="nPoints">The n points value.</param>
        /// <param name="nFaces">The n faces value.</param>
        public void Read(BinaryReaderEx input, int nPoints, int nFaces)
        {
            Points = new byte[nPoints];
            for (int index = 0; index < nPoints; ++index)
                Points[index] = input.ReadByte();
            Faces = new byte[nFaces];
            for (int index = 0; index < nFaces; ++index)
                Faces[index] = input.ReadByte();
        }
        /// <summary>Performs the write operation.</summary>
        /// <param name="output">The destination stream or writer.</param>
        public override void Write(BinaryWriterEx output)
        {
            WriteHeader(output);
            for (int index = 0; index < Points.Length; ++index)
                output.Write(Points[index]);
            for (int index = 0; index < Faces.Length; ++index)
                output.Write(Faces[index]);
        }
    }

    /// <summary>Represents property tagg.</summary>
    public class PropertyTagg : Tagg
    {
        /// <summary>Gets or sets the property name.</summary>
        public string PropertyName { get; set; }
        /// <summary>Gets or sets the value.</summary>
        public string Value { get; set; }

        /// <summary>Initializes a new PropertyTagg instance.</summary>
        /// <param name="prop">The prop value.</param>
        /// <param name="val">The val value.</param>
        public PropertyTagg(string prop, string val) : base(128,"#Property#")
        {
            PropertyName = prop;
            Value = val;
        }

        /// <summary>Initializes a new PropertyTagg instance.</summary>
        /// <param name="input">The source stream or value.</param>
        public PropertyTagg(BinaryReaderEx input) : base(input)
        {
            Read(input);
        }

        /// <summary>Performs the read operation.</summary>
        /// <param name="input">The source stream or value.</param>
        public void Read(BinaryReaderEx input)
        {
            PropertyName = input.ReadAscii(64);
            Value = input.ReadAscii(64);
        }

        /// <summary>Performs the write operation.</summary>
        /// <param name="output">The destination stream or writer.</param>
        public override void Write(BinaryWriterEx output)
        {
            WriteHeader(output);
            output.WriteAscii(PropertyName, 64);
            output.WriteAscii(Value, 64);
        }
    }
    /// <summary>Represents selected tagg.</summary>
    public class SelectedTagg : Tagg
    {
        /// <summary>Gets or sets the weighted points.</summary>
        public byte[] WeightedPoints { get; set; }
        /// <summary>Gets or sets the faces.</summary>
        public byte[] Faces { get; set; }

        /// <summary>Initializes a new SelectedTagg instance.</summary>
        /// <param name="input">The source stream or value.</param>
        /// <param name="nPoints">The n points value.</param>
        /// <param name="nFaces">The n faces value.</param>
        public SelectedTagg(BinaryReaderEx input, int nPoints, int nFaces) : base(input)
        {
            Read(input, nPoints, nFaces);
        }

        /// <summary>Performs the read operation.</summary>
        /// <param name="input">The source stream or value.</param>
        /// <param name="nPoints">The n points value.</param>
        /// <param name="nFaces">The n faces value.</param>
        public void Read(BinaryReaderEx input, int nPoints, int nFaces)
        {
            WeightedPoints = new byte[nPoints];
            for (int index = 0; index < nPoints; ++index)
                WeightedPoints[index] = input.ReadByte();
            Faces = new byte[nFaces];
            for (int index = 0; index < nFaces; ++index)
                Faces[index] = input.ReadByte();
        }

        /// <summary>Performs the write operation.</summary>
        /// <param name="output">The destination stream or writer.</param>
        public override void Write(BinaryWriterEx output)
        {
            WriteHeader(output);
            for (int index = 0; index < WeightedPoints.Length; ++index)
                output.Write(WeightedPoints[index]);
            for (int index = 0; index < Faces.Length; ++index)
                output.Write(Faces[index]);
        }
    }
    /// <summary>Represents sharp edges tagg.</summary>
    public class SharpEdgesTagg : Tagg
    {
        /// <summary>Gets the point indices.</summary>
        public int[,] PointIndices { get; private set; }

        /// <summary>Initializes a new SharpEdgesTagg instance.</summary>
        /// <param name="input">The source stream or value.</param>
        public SharpEdgesTagg(BinaryReaderEx input) : base(input)
        {
            Read(input);
        }

        /// <summary>Performs the read operation.</summary>
        /// <param name="input">The source stream or value.</param>
        public void Read(BinaryReaderEx input)
        {
            var num = DataSize / 8;
            PointIndices = new int[num, 2];
            for (int index = 0; index < num; ++index)
            {
                PointIndices[index, 0] = input.ReadInt32();
                PointIndices[index, 1] = input.ReadInt32();
            }
        }

        /// <summary>Performs the write operation.</summary>
        /// <param name="output">The destination stream or writer.</param>
        public override void Write(BinaryWriterEx output)
        {
            WriteHeader(output);
            var num = DataSize / 8;
            for (int index = 0; index < num; ++index)
            {
                output.Write(PointIndices[index, 0]);
                output.Write(PointIndices[index, 1]);
            }
        }
    }

    /// <summary>Represents uv set tagg.</summary>
    public class UVSetTagg : Tagg
    {
        /// <summary>Gets or sets the uv set nr.</summary>
        public int UvSetNr { get; set; }
        /// <summary>Gets or sets the face u vs.</summary>
        public float[][,] FaceUVs { get; set; }

        /// <summary>Initializes a new UVSetTagg instance.</summary>
        /// <param name="dataSize">The data size value.</param>
        /// <param name="uvNr">The uv nr value.</param>
        /// <param name="uvs">The uvs value.</param>
        public UVSetTagg(uint dataSize, int uvNr, float[][,] uvs): base(dataSize, "#UVSet#")
        {
            UvSetNr = uvNr;
            FaceUVs = uvs;
        }

        /// <summary>Initializes a new UVSetTagg instance.</summary>
        /// <param name="input">The source stream or value.</param>
        /// <param name="faces">The faces value.</param>
        public UVSetTagg(BinaryReaderEx input, Face[] faces) : base(input)
        {
            Read(input, faces);
        }

        /// <summary>Performs the read operation.</summary>
        /// <param name="input">The source stream or value.</param>
        /// <param name="faces">The faces value.</param>
        public void Read(BinaryReaderEx input, Face[] faces)
        {
            UvSetNr = input.ReadInt32();
            FaceUVs = new float[faces.Length][,];
            for (int i = 0; i < faces.Length; ++i)
            {
                FaceUVs[i] = new float[faces[i].VertexCount, 2];
                for (int j = 0; j < faces[i].VertexCount; ++j)
                {
                    FaceUVs[i][j, 0] = input.ReadSingle();
                    FaceUVs[i][j, 1] = input.ReadSingle();
                }
            }
        }

        /// <summary>Performs the write operation.</summary>
        /// <param name="output">The destination stream or writer.</param>
        public override void Write(BinaryWriterEx output)
        {
            WriteHeader(output);
            output.Write(UvSetNr);
            for (int i = 0; i < FaceUVs.Length; ++i)
            {
                for (int j = 0; j < FaceUVs[i].Length / 2; ++j)
                {
                    output.Write(FaceUVs[i][j, 0]);
                    output.Write(FaceUVs[i][j, 1]);
                }
            }
        }
    }

    /// <summary>Represents eof tagg.</summary>
    public class EOFTagg : Tagg
    {
        /// <summary>Initializes a new EOFTagg instance.</summary>
        public EOFTagg(): base(0, "#EndOfFile#") {}

        /// <summary>Initializes a new EOFTagg instance.</summary>
        /// <param name="input">The source stream or value.</param>
        public EOFTagg(BinaryReaderEx input) : base(input) {}

        /// <summary>Performs the write operation.</summary>
        /// <param name="output">The destination stream or writer.</param>
        public override void Write(BinaryWriterEx output)
        {
            WriteHeader(output);
        }
    }
}
