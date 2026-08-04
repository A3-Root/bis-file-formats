using BIS.Core.Math;
using BIS.Core.Streams;
using System;
using System.Collections.Generic;
using System.Text;

namespace BIS.WRP
{
    //public class RoadList
    //{
    //    private int nRoadLinks;
    //    private RoadLink[] roadLinks;

    //    public void Read(BinaryReaderEx input, int version)
    //    {
    //        nRoadLinks = input.ReadInt32();
    //        roadLinks = new RoadLink[nRoadLinks];
    //        for (int i = 0; i < nRoadLinks; i++)
    //        {
    //            roadLinks[i] = new RoadLink(input);
    //        }
    //    }
    //}

    /// <summary>Represents road link.</summary>
    public class RoadLink
    {
        /// <summary>Gets the connection count.</summary>
        public short ConnectionCount { get; }
        /// <summary>Gets the positions.</summary>
        public Vector3P[] Positions { get; }
        /// <summary>Gets the connection types.</summary>
        public byte[] ConnectionTypes { get; }
        /// <summary>Gets the object id.</summary>
        public int ObjectID { get; }
        /// <summary>Gets the p3d path.</summary>
        public string P3dPath { get; }
        /// <summary>Gets the to world.</summary>
        public Matrix4P ToWorld { get; }

        /// <summary>Initializes a new RoadLink instance.</summary>
        /// <param name="input">The source stream or value.</param>
        public RoadLink(BinaryReaderEx input)
        {
            ConnectionCount = input.ReadInt16();
            Positions = new Vector3P[ConnectionCount];
            for (int i = 0; i < ConnectionCount; i++)
                Positions[i] = new Vector3P(input);

            if (input.Version >= 24)
            {
                ConnectionTypes = new byte[ConnectionCount];
                for (int i = 0; i < ConnectionCount; i++)
                    ConnectionTypes[i] = input.ReadByte();
            }

            ObjectID = input.ReadInt32();

            if (input.Version >= 16)
            {
                P3dPath = input.ReadAsciiz();
                ToWorld = new Matrix4P(input);
            }
        }
    }
}
