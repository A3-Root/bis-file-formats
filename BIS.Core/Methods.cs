using System.Collections.Generic;
using System.Linq;

using static System.Math;

namespace BIS.Core
{
    /// <summary>Represents methods.</summary>
    public static class Methods
    {
        /// <summary>Swaps two values.</summary>
        /// <typeparam name="T">The t type.</typeparam>
        /// <param name="v1">The v1 value.</param>
        /// <param name="v2">The v2 value.</param>
        public static void Swap<T>(ref T v1, ref T v2)
        {
            var tmp = v1;
            v1 = v2;
            v2 = tmp;
        }

        /// <summary>Performs the equals float operation.</summary>
        /// <param name="f1">The f1 value.</param>
        /// <param name="f2">The f2 value.</param>
        /// <param name="tolerance">The tolerance value.</param>
        /// <returns>The resulting value.</returns>
        public static bool EqualsFloat(float f1, float f2, float tolerance = 0.0001f)
        {
            var dif = Abs(f1 - f2);
            if (dif <= tolerance) return true;
            return false;
        }

        /// <summary>Returns a sequence containing one value.</summary>
        /// <typeparam name="T">The t type.</typeparam>
        /// <param name="src">The src value.</param>
        /// <returns>The resulting values.</returns>
        public static IEnumerable<T> Yield<T>(this T src)
        {
            yield return src;
        }

        /// <summary>Returns the supplied values as a sequence.</summary>
        /// <typeparam name="T">The t type.</typeparam>
        /// <param name="elems">The elems value.</param>
        /// <returns>The resulting values.</returns>
        public static IEnumerable<T> Yield<T>(params T[] elems)
        {
            return elems;
        }

        /// <summary>Performs the chars to string operation.</summary>
        /// <param name="chars">The chars value.</param>
        /// <returns>The resulting value.</returns>
        public static string CharsToString(this IEnumerable<char> chars)
        {
            return new string(chars.ToArray());
        }
    }
}
