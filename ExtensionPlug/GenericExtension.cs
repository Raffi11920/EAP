using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Qynix.EAP.Utilities.ExtensionPlug
{
    public static class GenericExtension
    {
        /// <summary>
        /// Map a value in number range a to range b
        /// <para>Example:</para>
        /// <para>Find the value of 0 in range (-12 to 12) of (0 to 100)</para>
        /// <para>This gives 50.</para>
        /// </summary>
        /// <param name="value">The value to be calculate.</param>
        /// <param name="a1">Range a starting number.</param>
        /// <param name="a2">Range a ending number.</param>
        /// <param name="b1">Range b starting number.</param>
        /// <param name="b2">Range b ending number.</param>
        /// <returns></returns>
        public static float MapRange(this float value, float a1, float a2, float b1, float b2)
        {
            //return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
            return value * (b2 - a2) / (b1 - a1);
        }

        public static string ToDisplayString(this byte[] source)
        {
            return string.Join(" ", (from a in source select string.Format("{0:X2}", a))); ;
        }

        public static string ToIntegerString(this byte[] source)
        {
            return string.Join(" ", (from a in source select string.Format("{0}", a)));
        }

        public static string ToDisplayString(this IEnumerable<ushort> source)
        {
            return string.Join(" ", (from a in source select string.Format("{0}", a)));
        }

        public static string FillArguments(this string source, params object[] args)
        {
            if (source == null || args == null) return string.Empty;

            var result = string.Format(source, args);

            return result;
        }

        public static T ToEnum<T>(this string source)
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException("T must be enum type.");

            return (T)Enum.Parse(typeof(T), source, true);
        }

        public static int ToInt(this string source)
        {
            var parseInt = 0;

            if (int.TryParse(source, out parseInt))
            {
                return parseInt;
            }
            else
            {
                throw new ArgumentException("Invalid value to be parse as integer.");
            }
        }

        public static bool IsNumeric(this string source)
        {
            int tempLong = 0;

            return int.TryParse(source, out tempLong);
        }

        /// <summary>
        /// Get the array slice between the two indexes.
        /// ... Inclusive for start index, exclusive for end index.
        /// </summary>
        public static T[] Slice<T>(this T[] source, int start, int end)
        {
            // Handles negative ends.
            if (end < 0)
            {
                end = source.Length + end;
            }
            int len = end - start;

            // Return new array.
            T[] res = new T[len];
            for (int i = 0; i < len; i++)
            {
                res[i] = source[i + start];
            }
            return res;
        }

        /// <summary>
        /// Convert IEnumerable to Queue list.
        /// </summary>
        /// <typeparam name="T">Object type/class</typeparam>
        /// <param name="source">The caller.</param>
        /// <returns>Converted queue list.</returns>
        public static Queue<T> ToQueueList<T>(this IEnumerable<T> source)
        {
            Queue<T> queueList = new Queue<T>();
            var enumerator = source.GetEnumerator();
            enumerator.Reset();

            while (enumerator.MoveNext())
            {
                queueList.Enqueue(enumerator.Current);
            }

            return queueList;
        }

        public static void ClearAndNull<T>(this List<T> source)
        {
            source.Clear();
            source = null;
        }

        public static byte[] ToByteArray(this object source)
        {
            if (source == null)
                return null;

            var bf = new BinaryFormatter();
            var ms = new MemoryStream();

            bf.Serialize(ms, source);

            return ms.ToArray();
        }

        public static T ToObject<T>(this byte[] source)
        {
            var ms = new MemoryStream();

            var bf = new BinaryFormatter();
            ms.Write(source, 0, source.Length);

            ms.Seek(0, SeekOrigin.Begin);

            return (T)bf.Deserialize(ms);                        
        }

        public static IEnumerable<T> Append<T>(this IEnumerable<T> source, T obj)
        {
            if (source == null)
                return null;

            var list = source.ToList();

            list.Add(obj);

            return list;
        }
     }
}
