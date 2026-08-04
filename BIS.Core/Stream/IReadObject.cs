using System;
using System.Collections.Generic;
using System.Text;

namespace BIS.Core.Streams
{
    /// <summary>Defines operations for i read object.</summary>
    public interface IReadObject
    {
        /// <summary>Performs the read operation.</summary>
        /// <param name="input">The source stream or value.</param>
        void Read(BinaryReaderEx input);
    }
}
