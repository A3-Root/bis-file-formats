using System;
using System.Globalization;
using System.Numerics;
using BIS.Core.Streams;

namespace BIS.Core.Math
{
    /// <summary>Represents vector3 p.</summary>
    public class Vector3P
    {
        private Vector3 xyz;

        /// <summary>Gets or sets the x.</summary>
        public float X
        {
            get { return xyz.X; }
            set { xyz.X = value; }
        }

        /// <summary>Gets or sets the y.</summary>
        public float Y
        {
            get { return xyz.Y; }
            set { xyz.Y = value; }
        }

        /// <summary>Gets or sets the z.</summary>
        public float Z
        {
            get { return xyz.Z; }
            set { xyz.Z = value; }
        }

        /// <summary>Initializes a new Vector3P instance.</summary>
        public Vector3P() : this(0f) { }

        /// <summary>Initializes a new Vector3P instance.</summary>
        /// <param name="val">The val value.</param>
        public Vector3P(float val) : this(val, val, val) { }

        /// <summary>Initializes a new Vector3P instance.</summary>
        /// <param name="input">The source stream or value.</param>
        public Vector3P(BinaryReaderEx input) : this(input.ReadSingle(), input.ReadSingle(), input.ReadSingle()) { }

        /// <summary>Initializes a new Vector3P instance.</summary>
        /// <param name="compressed">The compressed value.</param>
        public Vector3P(int compressed) : this()
        {
            const double scaleFactor = -1.0 / 511;
            int x = compressed & 0x3FF;
            int y = (compressed >> 10) & 0x3FF;
            int z = (compressed >> 20) & 0x3FF;
            if (x > 511) x -= 1024;
            if (y > 511) y -= 1024;
            if (z > 511) z -= 1024;
            X = (float)(x * scaleFactor);
            Y = (float)(y * scaleFactor);
            Z = (float)(z * scaleFactor);
        }

        /// <summary>Initializes a new Vector3P instance.</summary>
        /// <param name="x">The x value.</param>
        /// <param name="y">The y value.</param>
        /// <param name="z">The z value.</param>
        public Vector3P(float x, float y, float z)
        {
            xyz = new Vector3( x, y, z );
        }

        /// <summary>Initializes a new Vector3P instance.</summary>
        /// <param name="xyz">The xyz value.</param>
        public Vector3P(Vector3 xyz)
        {
            this.xyz = xyz;
        }

        /// <summary>Stores the length value.</summary>
        public float Length => xyz.Length();

        /// <summary>Gets the vector3.</summary>
        public Vector3 Vector3 => xyz;

        /// <summary>Gets the this[int].</summary>
        /// <param name="i">The i value.</param>
        /// <returns>The resulting value.</returns>
        public float this[int i]
        {
            get
            {
                switch(i)
                {
                    case 0: return X;
                    case 1: return Y;
                    case 2: return Z;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(i));
                }
            }

            set
            {
                switch (i)
                {
                    case 0: X = value; break;
                    case 1: Y = value; break;
                    case 2: Z = value; break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(i));
                }
            }
        }

        /// <summary>Performs the operator - operation.</summary>
        /// <param name="a">The a value.</param>
        /// <returns>The resulting value.</returns>
        public static Vector3P operator -(Vector3P a)
        {
            return new Vector3P(-a.xyz);
        }

        /// <summary>Performs the write operation.</summary>
        /// <param name="output">The destination stream or writer.</param>
        public void Write(BinaryWriterEx output)
        {
            output.Write(X);
            output.Write(Y);
            output.Write(Z);
        }

        //Scalarmultiplication
        /// <summary>Performs the operator * operation.</summary>
        /// <param name="a">The a value.</param>
        /// <param name="b">The b value.</param>
        /// <returns>The resulting value.</returns>
        public static Vector3P operator *(Vector3P a, float b)
        {
            return new Vector3P(a.xyz * b);
        }

        //Scalarproduct
        /// <summary>Performs the operator * operation.</summary>
        /// <param name="a">The a value.</param>
        /// <param name="b">The b value.</param>
        /// <returns>The resulting value.</returns>
        public static float operator *(Vector3P a, Vector3P b)
        {
            return Vector3.Dot(a.xyz, b.xyz);
        }

        /// <summary>Performs the operator + operation.</summary>
        /// <param name="a">The a value.</param>
        /// <param name="b">The b value.</param>
        /// <returns>The resulting value.</returns>
        public static Vector3P operator +(Vector3P a, Vector3P b)
        {
            return new Vector3P(a.xyz + b.xyz);
        }

        /// <summary>Performs the operator - operation.</summary>
        /// <param name="a">The a value.</param>
        /// <param name="b">The b value.</param>
        /// <returns>The resulting value.</returns>
        public static Vector3P operator -(Vector3P a, Vector3P b)
        {
            return new Vector3P(a.xyz - b.xyz);
        }

        /// <summary>Performs the equals operation.</summary>
        /// <param name="obj">The obj value.</param>
        /// <returns>The resulting value.</returns>
        public override bool Equals(object obj)
        {
            Vector3P p = obj as Vector3P;
            if (p == null)
            {
                return false;
            }

            return base.Equals(obj) && Equals(p);
        }

        //ToDo:
        /// <summary>Gets hash code.</summary>
        /// <returns>The resulting value.</returns>
        public override int GetHashCode()
        {
            return xyz.GetHashCode();
        }

        /// <summary>Performs the equals operation.</summary>
        /// <param name="other">The other value.</param>
        /// <returns>The resulting value.</returns>
        public bool Equals(Vector3P other)
        {
            Func<float, float, bool> nearlyEqual = (f1, f2) => System.Math.Abs(f1 - f2) < 0.05;

            return ( nearlyEqual(X, other.X) && nearlyEqual(Y, other.Y) && nearlyEqual(Z, other.Z));
        }

        /// <summary>Converts this value to string.</summary>
        /// <returns>The resulting value.</returns>
        public override string ToString()
        {
            return "{" + X.ToString(CultureInfo.InvariantCulture) + "," + Y.ToString(CultureInfo.InvariantCulture) + "," + Z.ToString(CultureInfo.InvariantCulture) + "}";
        }

        /// <summary>Performs the distance operation.</summary>
        /// <param name="v">The v value.</param>
        /// <returns>The resulting value.</returns>
        public float Distance(Vector3P v)
        {
            return Vector3.Distance(xyz, v.xyz);
        }

        /// <summary>Performs the normalize operation.</summary>
        public void Normalize()
        {
            xyz = Vector3.Normalize(xyz);
        }


        /// <summary>Performs the cross product operation.</summary>
        /// <param name="a">The a value.</param>
        /// <param name="b">The b value.</param>
        /// <returns>The resulting value.</returns>
        public static Vector3P CrossProduct(Vector3P a, Vector3P b)
        {
            return new Vector3P(Vector3.Cross(a.xyz, b.xyz));
        }
    }
}
