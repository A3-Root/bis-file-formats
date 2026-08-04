using System;
using System.Collections.Generic;
using System.Text;

namespace BIS.Core.Streams
{
    /// <summary>Defines operations for i read write object.</summary>
    public interface IReadWriteObject : IReadObject
    {
        /// <summary>Performs the write operation.</summary>
        /// <param name="output">The destination stream or writer.</param>
        void Write(BinaryWriterEx output);
    }
}
