using BIS.Core.Streams;
using System;

namespace BIS.P3D.MLOD
{
    /// <summary>Specifies face flags values.</summary>
    [Flags]
    public enum FaceFlags
    {
        /// <summary>Specifies the default value.</summary>
        DEFAULT = 0,
        /// <summary>Specifies the nolight value.</summary>
        NOLIGHT = 0x1,
        /// <summary>Specifies the ambient value.</summary>
        AMBIENT = 0x2,
        /// <summary>Specifies the fulllight value.</summary>
        FULLLIGHT = 0x4,
        /// <summary>Specifies the bothsideslight value.</summary>
        BOTHSIDESLIGHT = 0x20,
        /// <summary>Specifies the skylight value.</summary>
        SKYLIGHT = 0x80,
        /// <summary>Specifies the reverselight value.</summary>
        REVERSELIGHT = 0x100000,
        /// <summary>Specifies the flatlight value.</summary>
        FLATLIGHT = 0x200000,
        /// <summary>Specifies the light mask value.</summary>
        LIGHT_MASK = 0x3000a7
    }

    /// <summary>Represents face.</summary>
    public class Face
    {
        /// <summary>Gets the vertex count.</summary>
        public int VertexCount { get; private set; }
        /// <summary>Gets the vertices.</summary>
        public Vertex[] Vertices { get; private set; }
        /// <summary>Gets the flags.</summary>
        public FaceFlags Flags { get; private set; }
        /// <summary>Gets the texture.</summary>
        public string Texture { get; private set; }
        /// <summary>Gets the material.</summary>
        public string Material { get; private set; }

        /// <summary>Initializes a new Face instance.</summary>
        /// <param name="nVerts">The n verts value.</param>
        /// <param name="verts">The verts value.</param>
        /// <param name="flags">The flags value.</param>
        /// <param name="texture">The texture value.</param>
        /// <param name="material">The material value.</param>
        public Face(int nVerts, Vertex[] verts, FaceFlags flags, string texture, string material)
        {
            VertexCount = nVerts;
            Vertices = verts;
            Flags = flags;
            Texture = texture;
            Material = material;
        }

        /// <summary>Initializes a new Face instance.</summary>
        /// <param name="input">The source stream or value.</param>
        public Face(BinaryReaderEx input)
        {
            Read(input);
        }

        /// <summary>Performs the read operation.</summary>
        /// <param name="input">The source stream or value.</param>
        public void Read(BinaryReaderEx input)
        {
            VertexCount = input.ReadInt32();
            Vertices = new Vertex[4];
            for (int i = 0; i < 4; ++i)
            {
                Vertices[i] = new Vertex(input);
            }
            Flags = (FaceFlags)input.ReadInt32();
            Texture = input.ReadAsciiz();
            Material = input.ReadAsciiz();
        }

        /// <summary>Performs the write operation.</summary>
        /// <param name="output">The destination stream or writer.</param>
        public void Write(BinaryWriterEx output)
        {
            output.Write(VertexCount);
            for (int i = 0; i < 4; ++i)
                if (i < Vertices.Length && Vertices[i] != null)
                    Vertices[i].Write(output);
                else
                {
                    output.Write(0);
                    output.Write(0);
                    output.Write(0);
                    output.Write(0);
                }

            output.Write((int)Flags);
            output.WriteAsciiz(Texture);
            output.WriteAsciiz(Material);
        }
    }
}
