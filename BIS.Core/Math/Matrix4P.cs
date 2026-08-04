using System;
using BIS.Core.Streams;

namespace BIS.Core.Math
{
    /// <summary>
    /// Layout:
    /// [m11, m12, m13, 0]
    /// [m21, m22, m23, 0]
    /// [m31, m32, m33, 0]
    /// [m41, m42, m43, 1]
    /// </summary>
    public class Matrix4P
    {
        private System.Numerics.Matrix4x4 matrix;

        /// <summary>Initializes a new Matrix4P instance.</summary>
        /// <param name="input">The source stream or value.</param>
        public Matrix4P(BinaryReaderEx input) : this(
            new System.Numerics.Matrix4x4(
                input.ReadSingle(), input.ReadSingle(), input.ReadSingle(), 0f,
                input.ReadSingle(), input.ReadSingle(), input.ReadSingle(), 0f,
                input.ReadSingle(), input.ReadSingle(), input.ReadSingle(), 0f,
                input.ReadSingle(), input.ReadSingle(), input.ReadSingle(), 1f)
            ) { 
        }

        /// <summary>Initializes a new Matrix4P instance.</summary>
        /// <param name="matrix">The matrix value.</param>
        public Matrix4P(System.Numerics.Matrix4x4 matrix)
        {
            this.matrix = matrix;
        }

        /// <summary>Performs the operator * operation.</summary>
        /// <param name="a">The a value.</param>
        /// <param name="b">The b value.</param>
        /// <returns>The resulting value.</returns>
        public static Matrix4P operator *(Matrix4P a, Matrix4P b)
        {
            return new Matrix4P(a.matrix * b.matrix);
        }

        /// <summary>Performs the write operation.</summary>
        /// <param name="output">The destination stream or writer.</param>
        public void Write(BinaryWriterEx output)
        {
            if (matrix.M14 != 0f || matrix.M24 != 0f || matrix.M34 != 0f || matrix.M44 != 1f)
            {
                throw new InvalidOperationException();
            }
            output.Write(matrix.M11);
            output.Write(matrix.M12);
            output.Write(matrix.M13);
            output.Write(matrix.M21);
            output.Write(matrix.M22);
            output.Write(matrix.M23);
            output.Write(matrix.M31);
            output.Write(matrix.M32);
            output.Write(matrix.M33);
            output.Write(matrix.M41);
            output.Write(matrix.M42);
            output.Write(matrix.M43);
        }

        /// <summary>Converts this value to string.</summary>
        /// <returns>The resulting value.</returns>
        public override string ToString()
        {
            return matrix.ToString();
        }
    }
}
