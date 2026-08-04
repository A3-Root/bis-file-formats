using BIS.Core.Math;
using BIS.Core.Streams;

namespace BIS.WRP
{
    /// <summary>Represents static entity info.</summary>
    public class StaticEntityInfo
    {
        /// <summary>Gets the class name.</summary>
        public string ClassName { get; }
        /// <summary>Gets the shape name.</summary>
        public string ShapeName { get; }
        /// <summary>Gets the position.</summary>
        public Vector3P Position { get; }
        /// <summary>Gets the object id.</summary>
        public ObjectId ObjectId { get; }

        /// <summary>Initializes a new StaticEntityInfo instance.</summary>
        /// <param name="input">The source stream or value.</param>
        public StaticEntityInfo(BinaryReaderEx input)
        {
            ClassName = input.ReadAsciiz();
            ShapeName = input.ReadAsciiz();
            Position = new Vector3P(input);
            ObjectId = input.ReadInt32();
        }
    }
}
