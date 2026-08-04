using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

using BIS.Core.Streams;
using BIS.Core.Math;

namespace BIS.RTM
{
    /// <summary>Represents rtm.</summary>
    public class RTM
    {
        /// <summary>Gets the displacement.</summary>
        public Vector3P Displacement { get; private set; }
        /// <summary>Gets the bone names.</summary>
        public string[] BoneNames { get; private set; }
        /// <summary>Gets the frame times.</summary>
        public float[] FrameTimes { get; private set; }
        /// <summary>Gets the frame transforms.</summary>
        public Matrix4P[,] FrameTransforms { get; private set; }

        /// <summary>Initializes a new RTM instance.</summary>
        /// <param name="fileName">The file path.</param>
        public RTM(string fileName) : this(File.OpenRead(fileName)) { }

        /// <summary>Initializes a new RTM instance.</summary>
        /// <param name="stream">The source stream or value.</param>
        public RTM(Stream stream)
        {
            var input = new BinaryReaderEx(stream);
            Read(input);
            input.Close();
        }

        private void Read(BinaryReaderEx input)
        {
            if ("RTM_0101" == input.ReadAscii(8))
            {
                ReadRTM(input);
                return;
            }
            throw new FormatException("No RTM signature found");
        }

        private void Write(BinaryWriterEx output)
        {
            output.WriteAscii("RTM_0101", 8);
            Displacement.Write(output);

            var nFrames = FrameTimes.Length;
            var nBones = BoneNames.Length;

            output.Write(nFrames);
            output.Write(nBones);

            for (int i = 0; i < nBones; i++)
                output.WriteAscii(BoneNames[i], 32);

            for (int frame = 0; frame < nFrames; frame++)
            {
                output.Write(FrameTimes[frame]); 
                for (int b = 0; b < nBones; b++)
                {
                    output.WriteAscii(BoneNames[b], 32);
                    FrameTransforms[frame, b].Write(output);
                }
            }
        }

        private void ReadRTM(BinaryReaderEx input)
        {
            Displacement = new Vector3P(input);
            var nFrames = input.ReadInt32();

            BoneNames = input.ReadArray( inp => inp.ReadAscii(32) );

            var nBones = BoneNames.Length;

            FrameTimes = new float[nFrames];
            FrameTransforms = new Matrix4P[nFrames, nBones];
            for (int frame = 0; frame < nFrames; frame++)
            {
                FrameTimes[frame] = input.ReadSingle();
                for (int b = 0; b < nBones; b++)
                {
                    input.ReadAscii(32); //redundand boneName
                    FrameTransforms[frame, b] = new Matrix4P(input);
                }
            }
        }

        /// <summary>Writes to file to the underlying data.</summary>
        /// <param name="file">The file value.</param>
        public void WriteToFile(string file)
        {
            var output = new BinaryWriterEx(File.OpenWrite(file));
            Write(output);
            output.Close();
        }
    }
}
