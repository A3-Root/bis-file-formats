using BIS.Core.Math;
using BIS.Core.Streams;
using System;
using System.Collections.Generic;
using System.Text;

namespace BIS.WRP
{
    /// <summary>Represents object.</summary>
    public class Object
    {
        /// <summary>Gets the object id.</summary>
        public int ObjectID { get; }
        /// <summary>Gets the model index.</summary>
        public int ModelIndex { get; } // into the [[#Models|models path name list]] (1 based)
        /// <summary>Gets the transform.</summary>
        public Matrix4P Transform { get; }
        /// <summary>Gets the shape param.</summary>
        public int ShapeParam { get; }

        /// <summary>Initializes a new Object instance.</summary>
        /// <param name="input">The source stream or value.</param>
        public Object(BinaryReaderEx input)
        {
            ObjectID = input.ReadInt32();
            ModelIndex = input.ReadInt32();
            Transform = new Matrix4P(input);
            if (input.Version >= 14)
                ShapeParam = input.ReadInt32();
        }
    }
}
