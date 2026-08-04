using System;
using System.Collections.Generic;
using System.Text;

namespace BIS.WRP
{
    /// <summary>Represents object id.</summary>
    public struct ObjectId
    {
        private int id;

        /// <summary>Stores the is object value.</summary>
        public bool IsObject => ((id >> 31) & 1) > 0;
        /// <summary>Stores the obj id value.</summary>
        public short ObjId => (short)(id & 0b111_1111_1111);
        /// <summary>Stores the obj x value.</summary>
        public short ObjX => (short)((id >> 11) & 0b11_1111_1111);
        /// <summary>Stores the obj z value.</summary>
        public short ObjZ => (short)((id >> 21) & 0b11_1111_1111);

        /// <summary>Stores the id value.</summary>
        public int Id => id;

        /// <summary>Performs the implicit operator int operation.</summary>
        /// <param name="d">The d value.</param>
        /// <returns>The resulting value.</returns>
        public static implicit operator int(ObjectId d)
        {
            return d.id;
        }

        /// <summary>Performs the implicit operator object id operation.</summary>
        /// <param name="d">The d value.</param>
        /// <returns>The resulting value.</returns>
        public static implicit operator ObjectId(int d)
        {
            var o = new ObjectId
            {
                id = d
            };
            return o;
        }
    }
}
