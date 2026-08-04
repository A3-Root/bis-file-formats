using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using BIS.Core.Math;
using BIS.Core.Streams;

namespace BIS.WRP
{
    /// <summary>Represents editable wrp object.</summary>
    public class EditableWrpObject
    {
        /// <summary>Stores the dummy value.</summary>
        public static EditableWrpObject Dummy = new EditableWrpObject()
        {
            Model = "",
            ObjectID = int.MaxValue, 
            Transform = new Matrix4P(new Matrix4x4(
                        float.NaN, float.NaN, float.NaN, 0f,
                        float.NaN, float.NaN, float.NaN, 0f,
                        float.NaN, float.NaN, float.NaN, 0f,
                        float.NaN, float.NaN, float.NaN, 1f))
        };

        /// <summary>Initializes a new EditableWrpObject instance.</summary>
        public EditableWrpObject()
        {

        }

        /// <summary>Initializes a new EditableWrpObject instance.</summary>
        /// <param name="input">The source stream or value.</param>
        public EditableWrpObject(BinaryReaderEx input)
        {
            Transform = new Matrix4P(input);
            ObjectID = input.ReadInt32();
            Model = input.ReadAscii32();
        }

        /// <summary>Gets or sets the transform.</summary>
        public Matrix4P Transform { get; set; }
        /// <summary>Gets or sets the object id.</summary>
        public int ObjectID { get; set; }
        /// <summary>Gets or sets the model.</summary>
        public string Model { get; set; }

        internal void Write(BinaryWriterEx output)
        {
            Transform.Write(output);
            output.Write(ObjectID);
            output.WriteAscii32(Model);
        }
    }
}
