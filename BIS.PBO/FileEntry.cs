using BIS.Core.Streams;
using System;
using System.Collections.Generic;
using System.Text;

namespace BIS.PBO
{
    /// <summary>Represents file entry.</summary>
    public class FileEntry
    {
        /// <summary>Gets or sets the file name.</summary>
        public string FileName { get; set; }
        /// <summary>Gets or sets the compressed magic.</summary>
        public int CompressedMagic { get; set; }
        /// <summary>Gets or sets the uncompressed size.</summary>
        public int UncompressedSize { get; set; }
        /// <summary>Gets or sets the start offset.</summary>
        public int StartOffset { get; set; }
        /// <summary>Gets or sets the time stamp.</summary>
        public int TimeStamp { get; set; }
        /// <summary>Gets or sets the data size.</summary>
        public int DataSize { get; set; }

        /// <summary>Stores the version magic value.</summary>
        public static int VersionMagic = BitConverter.ToInt32(Encoding.ASCII.GetBytes("sreV"), 0); //Vers
        /// <summary>Stores the compression magic value.</summary>
        public static int CompressionMagic = BitConverter.ToInt32(Encoding.ASCII.GetBytes("srpC"), 0); //Cprs
        /// <summary>Stores the encryption magic value.</summary>
        public static int EncryptionMagic = BitConverter.ToInt32(Encoding.ASCII.GetBytes("rcnE"), 0); //Encr

        /// <summary>Initializes a new FileEntry instance.</summary>
        public FileEntry()
        {
            FileName = "";
            CompressedMagic = 0;
            UncompressedSize = 0;
            StartOffset = 0;
            TimeStamp = 0;
            DataSize = 0;
        }
        /// <summary>Initializes a new FileEntry instance.</summary>
        /// <param name="input">The source stream or value.</param>
        public FileEntry(BinaryReaderEx input)
        {
            Read(input);
        }

        /// <summary>Performs the read operation.</summary>
        /// <param name="input">The source stream or value.</param>
        public void Read(BinaryReaderEx input)
        {
            FileName = input.ReadAsciiz();
            CompressedMagic = input.ReadInt32();
            UncompressedSize = input.ReadInt32();
            StartOffset = input.ReadInt32();
            TimeStamp = input.ReadInt32();
            DataSize = input.ReadInt32();
        }

        /// <summary>Performs the write operation.</summary>
        /// <param name="output">The destination stream or writer.</param>
        public void Write(BinaryWriterEx output)
        {
            output.WriteAsciiz(FileName);
            output.Write(CompressedMagic);
            output.Write(UncompressedSize);
            output.Write(StartOffset);
            output.Write(TimeStamp);
            output.Write(DataSize);
        }

        /// <summary>Stores the is version value.</summary>
        public bool IsVersion => CompressedMagic == VersionMagic && TimeStamp == 0 && DataSize == 0;
        /// <summary>Stores the is compressed value.</summary>
        public bool IsCompressed => CompressedMagic == CompressionMagic;
    }
}
