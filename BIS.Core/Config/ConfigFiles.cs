using BIS.Core.Streams;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BIS.Core.Config
{
    /// <summary>Represents param file.</summary>
    public class ParamFile : IReadObject
    {
        /// <summary>Gets the root.</summary>
        public ParamClass Root { get; private set; }
        /// <summary>Gets the enum values.</summary>
        public List<KeyValuePair<string, int>> EnumValues { get; private set; }

        /// <summary>Initializes a new ParamFile instance.</summary>
        public ParamFile()
        {
            EnumValues = new List<KeyValuePair<string, int>>(10);
        }

        /// <summary>Initializes a new ParamFile instance.</summary>
        /// <param name="stream">The source stream or value.</param>
        public ParamFile(System.IO.Stream stream)
        {
            Read(new BinaryReaderEx(stream));
        }

        /// <summary>Performs the read operation.</summary>
        /// <param name="input">The source stream or value.</param>
        public void Read(BinaryReaderEx input)
        {
            var sig = new char[] { '\0', 'r', 'a', 'P' };
            if (!input.ReadBytes(4).SequenceEqual(sig.Select(c => (byte)c)))
                throw new ArgumentException();

            var ofpVersion = input.ReadInt32();
            var version = input.ReadInt32();
            var offsetToEnums = input.ReadInt32();

            Root = new ParamClass(input, "rootClass");

            input.Position = offsetToEnums;
            var nEnumValues = input.ReadInt32();
            EnumValues = Enumerable.Range(0, nEnumValues).Select(_ => new KeyValuePair<string, int>(input.ReadAsciiz(), input.ReadInt32())).ToList();
        }

        /// <summary>Converts this value to string.</summary>
        /// <returns>The resulting value.</returns>
        public override string ToString()
        {
            return Root.ToString(0, true);
        }
    }
}
