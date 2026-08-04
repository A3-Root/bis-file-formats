using BIS.Core.Streams;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIS.ALB
{
    /// <summary>Represents object tree node.</summary>
    public class ObjectTreeNode
    {
        /// <summary>Gets the node type.</summary>
        public sbyte NodeType { get; }
        /// <summary>Gets the area.</summary>
        public MapArea Area { get; }
        /// <summary>Gets the level.</summary>
        public int Level { get; }
        /// <summary>Gets the color.</summary>
        public byte[] Color { get; }
        /// <summary>Stores the flags value.</summary>
        public byte flags;

        /// <summary>Stores the childs value.</summary>
        public ObjectTreeNode[] Childs;

        /// <summary>Stores the objects value.</summary>
        public ObjectTreeLeaf[] Objects;

        /// <summary>Initializes a new ObjectTreeNode instance.</summary>
        /// <param name="input">The source stream or value.</param>
        /// <param name="layerVersion">The layer version value.</param>
        public ObjectTreeNode(BinaryReaderEx input, int layerVersion)
        {
            NodeType = input.ReadSByte();

            Area = new MapArea(input, layerVersion >= 4);

            Level = input.ReadInt32();
            Color = Enumerable.Range(0, 4).Select(_ => input.ReadByte()).ToArray();
            flags = input.ReadByte();

            if (NodeType == 16)
            {
                Objects = new ObjectTreeLeaf[4];
                var isChild = flags;
                for (int i = 0; i < 4; i++)
                {
                    if ((isChild & 1) == 1) Objects[i] = new ObjectTreeLeaf(input, layerVersion);
                    isChild >>= 1;
                }
            }
            else
            {
                Childs = new ObjectTreeNode[4];
                var isChild = flags;
                for (int i = 0; i < 4; i++)
                {
                    if ((isChild & 1) == 1) Childs[i] = new ObjectTreeNode(input, layerVersion);
                    isChild >>= 1;
                }
            }
        }
    }

    /// <summary>Represents object tree leaf.</summary>
    public class ObjectTreeLeaf
    {
        /// <summary>Gets the area.</summary>
        public MapArea Area { get; }
        /// <summary>Gets the color.</summary>
        public byte[] Color { get; }

        //it's currently not clear what object hash is stored here; maybe the one covering the most area
        /// <summary>Gets the hash value.</summary>
        public int HashValue { get; }
        /// <summary>Gets the object type count.</summary>
        public int ObjectTypeCount { get; }
        /// <summary>Gets the object type hashes.</summary>
        public int[] ObjectTypeHashes { get; }
        /// <summary>Gets the object infos.</summary>
        public ObjectInfo[][] ObjectInfos { get; }

        /// <summary>Initializes a new ObjectTreeLeaf instance.</summary>
        /// <param name="input">The source stream or value.</param>
        /// <param name="layerVersion">The layer version value.</param>
        public ObjectTreeLeaf(BinaryReaderEx input, int layerVersion)
        {
            Area = new MapArea(input, layerVersion >= 4);
            Color = input.ReadBytes(4);
            HashValue = input.ReadInt32();
            ObjectTypeCount = input.ReadInt32();

            ObjectTypeHashes = new int[ObjectTypeCount];
            ObjectInfos = new ObjectInfo[ObjectTypeCount][];
            for(int curObjType = 0; curObjType < ObjectTypeCount; curObjType++)
            {
                var nObjects = input.ReadInt32();
                ObjectTypeHashes[curObjType] = input.ReadInt32();
                ObjectInfos[curObjType] = new ObjectInfo[nObjects];
                for (int obj = 0; obj < nObjects; obj++)
                {
                    ObjectInfos[curObjType][obj] = new ObjectInfo(input);
                }
            }
        }

        /// <summary>Converts this value to string.</summary>
        /// <returns>The resulting value.</returns>
        public override string ToString()
        {
            var node = $"{Area};{HashValue}:";
            var sb = new StringBuilder(node);
            sb.AppendLine();
            for(int i=0;i < ObjectTypeCount; i++)
            {
                var objType = ObjectTypeHashes[i];
                foreach(var objinfo in ObjectInfos[i])
                {
                    sb.AppendLine($"    {objType};{objinfo}");
                }
            }

            return sb.ToString();
        }
    }

    /// <summary>Represents object info.</summary>
    public class ObjectInfo
    {
        /// <summary>Gets the x.</summary>
        public double X { get; }
        /// <summary>Gets the y.</summary>
        public double Y { get; }
        /// <summary>Gets the yaw.</summary>
        public float Yaw { get; }
        /// <summary>Gets the pitch.</summary>
        public float Pitch { get; }
        /// <summary>Gets the roll.</summary>
        public float Roll { get; }
        /// <summary>Gets the scale.</summary>
        public float Scale { get; }
        /// <summary>Gets the relative elevation.</summary>
        public float RelativeElevation { get; }
        /// <summary>Gets the id.</summary>
        public int ID { get; }

        /// <summary>Initializes a new ObjectInfo instance.</summary>
        /// <param name="input">The source stream or value.</param>
        public ObjectInfo(BinaryReaderEx input)
        {
            X = input.ReadDouble();
            Y = input.ReadDouble();
            Yaw = input.ReadSingle();
            Pitch = input.ReadSingle();
            Roll = input.ReadSingle();
            Scale = input.ReadSingle();
            RelativeElevation = input.ReadSingle();
            ID = input.ReadInt32();
        }

        /// <summary>Converts this value to string.</summary>
        /// <returns>The resulting value.</returns>
        public override string ToString()
        {
            return $"{X:0.###};{Y:0.###};{Yaw:0.###};{Pitch:0.###};{Roll:0.###};{Scale:0.###};{RelativeElevation:0.###};{ID}";
        }
    }
}
