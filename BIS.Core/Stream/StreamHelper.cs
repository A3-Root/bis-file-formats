using System.IO;

namespace BIS.Core.Streams
{
    /// <summary>Provides convenience methods for reading and writing BI objects.</summary>
    public static class StreamHelper
    {
        private static MemoryStream MakeBuffer(Stream stream)
        {
            var ms = new MemoryStream((int)stream.Length);
            stream.CopyTo(ms);
            ms.Position = 0;
            return ms;
        }

        /// <summary>Reads an object from a buffered copy of a stream.</summary>
        /// <typeparam name="T">The object type.</typeparam>
        /// <param name="input">The source stream.</param>
        /// <returns>The decoded object.</returns>
        public static T Read<T> (Stream input) where T : IReadObject, new()
        {
            var o = new T();
            o.Read(new BinaryReaderEx(MakeBuffer(input)));
            return o;
        }

        /// <summary>Reads an object from a buffered copy of a file.</summary>
        /// <typeparam name="T">The object type.</typeparam>
        /// <param name="filename">The source file path.</param>
        /// <returns>The decoded object.</returns>
        public static T Read<T>(string filename) where T : IReadObject, new()
        {
            using(var input = File.OpenRead(filename))
            {
                return Read<T>(input);
            }
        }

        /// <summary>Reads an object from the supplied stream.</summary>
        /// <typeparam name="T">The object type.</typeparam>
        /// <param name="input">The source stream.</param>
        /// <returns>The decoded object.</returns>
        public static T ReadNoBuffer<T>(Stream input) where T : IReadObject, new()
        {
            var o = new T();
            o.Read(new BinaryReaderEx(MakeBuffer(input)));
            return o;
        }

        /// <summary>Reads an object from a file.</summary>
        /// <typeparam name="T">The object type.</typeparam>
        /// <param name="filename">The source file path.</param>
        /// <returns>The decoded object.</returns>
        public static T ReadNoBuffer<T>(string filename) where T : IReadObject, new()
        {
            using (var input = File.OpenRead(filename))
            {
                return Read<T>(input);
            }
        }

        /// <summary>Writes an object to a file.</summary>
        /// <typeparam name="T">The object type.</typeparam>
        /// <param name="value">The object to encode.</param>
        /// <param name="filename">The destination file path.</param>
        public static void Write<T>(this T value, string filename) where T : IReadWriteObject
        {
            using (var output = new FileStream(filename, FileMode.Create, FileAccess.Write))
            {
                Write<T>(value, output);
            }
        }

        /// <summary>Writes an object to a stream.</summary>
        /// <typeparam name="T">The object type.</typeparam>
        /// <param name="value">The object to encode.</param>
        /// <param name="stream">The destination stream.</param>
        public static void Write<T>(this T value, Stream stream) where T : IReadWriteObject
        {
            value.Write(new BinaryWriterEx(stream));
        }
    }
}
