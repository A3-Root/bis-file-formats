using BIS.Core.Streams;
using System;
using System.IO;

namespace BIS.P3D.MLOD
{
    /// <summary>Represents mlod.</summary>
    public class MLOD
    {
        /// <summary>Gets the version.</summary>
        public int Version { get; private set; }
        /// <summary>Gets the lods.</summary>
        public P3DM_LOD[] Lods { get; private set; }

        /// <summary>Initializes a new MLOD instance.</summary>
        /// <param name="fileName">The file path.</param>
        public MLOD(string fileName) : this(File.OpenRead(fileName)) {}

        /// <summary>Initializes a new MLOD instance.</summary>
        /// <param name="stream">The source stream or value.</param>
        public MLOD(Stream stream)
        {
            Read(new BinaryReaderEx(stream));
        }

        /// <summary>Initializes a new MLOD instance.</summary>
        /// <param name="lods">The lods value.</param>
        public MLOD(P3DM_LOD[] lods)
        {
            Version = 257;
            Lods = lods;
        }

        private void Read(BinaryReaderEx input)
        {
            if (input.ReadAscii(4) != "MLOD")
                throw new FormatException("MLOD signature expected");

            Version = input.ReadInt32();
            if (Version != 257)
                throw new ArgumentException("Unknown MLOD version");

            Lods = input.ReadArray(inp => new P3DM_LOD(inp));
        }

        private void Write(BinaryWriterEx output)
        {
            output.WriteAscii("MLOD", 4);
            output.Write(Version);
            output.Write(Lods.Length);
            for (int index = 0; index < Lods.Length; ++index)
                Lods[index].Write(output);
        }

        /// <summary>Writes to file to the underlying data.</summary>
        /// <param name="file">The file value.</param>
        /// <param name="allowOverwriting">The allow overwriting value.</param>
        public void WriteToFile(string file, bool allowOverwriting=false)
        {
            var mode = (allowOverwriting) ? FileMode.Create : FileMode.CreateNew;

            var fs = new FileStream(file, mode);
            using (var output = new BinaryWriterEx(fs))
            {
                Write(output);
            }
        }

        /// <summary>Writes to memory to the underlying data.</summary>
        /// <returns>The resulting value.</returns>
        public MemoryStream WriteToMemory()
        {
            var memStream = new MemoryStream(100000);
            var outStream = new BinaryWriterEx(memStream);
            Write(outStream);
            outStream.Position = 0;
            return memStream;
        }

        /// <summary>Writes to stream to the underlying data.</summary>
        /// <param name="stream">The source stream or value.</param>
        public void WriteToStream(Stream stream)
        {
            var output = new BinaryWriterEx(stream);
            Write(output);
        }
    }
}
