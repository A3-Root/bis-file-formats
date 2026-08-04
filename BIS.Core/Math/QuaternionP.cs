using System;
using System.IO;
using System.Numerics;

namespace BIS.Core.Math
{
    /// <summary>Represents quaternion p.</summary>
    public class QuaternionP
    {
        private System.Numerics.Quaternion quaternion;

        /// <summary>Stores the x value.</summary>
        public float X => quaternion.X;
        /// <summary>Stores the y value.</summary>
        public float Y => quaternion.Y;
        /// <summary>Stores the z value.</summary>
        public float Z => quaternion.Z;
        /// <summary>Stores the w value.</summary>
        public float W => quaternion.W;

        /// <summary>Reads compressed from the underlying data.</summary>
        /// <param name="input">The source stream or value.</param>
        /// <returns>The resulting value.</returns>
        public static QuaternionP ReadCompressed(BinaryReader input)
        {
            var x = (float)(input.ReadInt16() / 16384d);
            var y = (float)(input.ReadInt16() / 16384d);
            var z = (float)(input.ReadInt16() / 16384d);
            var w = (float)(input.ReadInt16() / 16384d);

            return new QuaternionP(x, y, z, w);
        }

        /// <summary>Initializes a new QuaternionP instance.</summary>
        public QuaternionP() 
            : this(System.Numerics.Quaternion.Identity)
        {
        }

        /// <summary>Initializes a new QuaternionP instance.</summary>
        /// <param name="x">The x value.</param>
        /// <param name="y">The y value.</param>
        /// <param name="z">The z value.</param>
        /// <param name="w">The w value.</param>
        public QuaternionP(float x, float y, float z, float w)
            : this(new System.Numerics.Quaternion(x, y, z, w))
        {

        }

        /// <summary>Initializes a new QuaternionP instance.</summary>
        /// <param name="quaternion">The quaternion value.</param>
        public QuaternionP(System.Numerics.Quaternion quaternion)
        {
            this.quaternion = quaternion;
        }

        /// <summary>Performs the operator * operation.</summary>
        /// <param name="a">The a value.</param>
        /// <param name="b">The b value.</param>
        /// <returns>The resulting value.</returns>
        public static QuaternionP operator *(QuaternionP a, QuaternionP b)
        {
            return new QuaternionP(a.quaternion * b.quaternion);
        }

        /// <summary>Gets the inverse.</summary>
        public QuaternionP Inverse
        {
            get
            {
                Normalize();
                return Conjugate;
            }
        }

        /// <summary>Stores the conjugate value.</summary>
        public QuaternionP Conjugate => new QuaternionP(System.Numerics.Quaternion.Conjugate(quaternion));

        /// <summary>Performs the normalize operation.</summary>
        public void Normalize()
        {
            quaternion = System.Numerics.Quaternion.Normalize(quaternion);
        }

        /// <summary>Performs the transform operation.</summary>
        /// <param name="xyz">The xyz value.</param>
        /// <returns>The resulting value.</returns>
        public Vector3P Transform(Vector3P xyz)
        {
            return new Vector3P(System.Numerics.Vector3.Transform(xyz.Vector3, quaternion));
        }

        /// <summary>
        /// for unit quaternions only?
        /// </summary>
        /// <returns></returns>
        public Matrix4P ToRotationMatrix()
        {
            return new Matrix4P(Matrix4x4.CreateFromQuaternion(quaternion)); 
        }
    }
}
