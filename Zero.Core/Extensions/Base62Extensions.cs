using Newtonsoft.Json;
using System.Collections.Generic;
using System.Globalization;

using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace Zero.Core.Extensions
{
    /// <summary>
    /// base62拓展
    /// </summary>
    public static class Base62Extensions
    {
        private const string DefaultCharacterSet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        private const string InvertedCharacterSet = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

        /// <summary>
        /// Encode a 2-byte number with Base62
        /// </summary>
        /// <param name="original">String</param>
        /// <param name="inverted">Use inverted character set</param>
        /// <returns>Base62 string</returns>
        public static string ToBase62(this short original, bool inverted = false)
        {
            var array = BitConverter.GetBytes(original);

            var length = array.Length;

            for (var i = length - 1; i > 0; i--)
            {
                if (array[i] == 0)
                {
                    length--;
                    continue;
                }
                break;
            }

            array = array.Take(length).ToArray();

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(array);
            }

            return array.ToBase62(inverted);
        }

        /// <summary>
        /// Encode a 4-byte number with Base62
        /// </summary>
        /// <param name="original">String</param>
        /// <param name="inverted">Use inverted character set</param>
        /// <returns>Base62 string</returns>
        public static string ToBase62(this int original, bool inverted = false)
        {
            var array = BitConverter.GetBytes(original);

            var length = array.Length;

            for (var i = length - 1; i > 0; i--)
            {
                if (array[i] == 0)
                {
                    length--;
                    continue;
                }
                break;
            }

            array = array.Take(length).ToArray();

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(array);
            }

            return array.ToBase62(inverted);
        }

        /// <summary>
        /// Encode a 8-byte number with Base62
        /// </summary>
        /// <param name="original">String</param>
        /// <param name="inverted">Use inverted character set</param>
        /// <returns>Base62 string</returns>
        public static string ToBase62(this long original, bool inverted = false)
        {
            var array = BitConverter.GetBytes(original);

            var length = array.Length;

            for (var i = length - 1; i > 0; i--)
            {
                if (array[i] == 0)
                {
                    length--;
                    continue;
                }
                break;
            }

            array = array.Take(length).ToArray();

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(array);
            }

            return array.ToBase62(inverted);
        }

        /// <summary>
        /// Encode a string with Base62
        /// </summary>
        /// <param name="original">String</param>
        /// <param name="inverted">Use inverted character set</param>
        /// <returns>Base62 string</returns>
        public static string ToBase62(this string original, bool inverted = false)
        {
            return Encoding.UTF8.GetBytes(original).ToBase62(inverted);
        }

        /// <summary>
        /// Encode a byte array with Base62
        /// </summary>
        /// <param name="original">Byte array</param>
        /// <param name="inverted">Use inverted character set</param>
        /// <returns>Base62 string</returns>
        public static string ToBase62(this byte[] original, bool inverted = false)
        {
            var characterSet = inverted ? InvertedCharacterSet : DefaultCharacterSet;
            var arr = Array.ConvertAll(original, t => (int)t);

            var converted = BaseConvert(arr, 256, 62);
            var builder = new StringBuilder();
            foreach (var t in converted)
            {
                builder.Append(characterSet[t]);
            }
            return builder.ToString();
        }

        /// <summary>
        /// Decode a base62-encoded string
        /// </summary>
        /// <param name="base62">Base62 string</param>
        /// <param name="inverted">Use inverted character set</param>
        /// <returns>Byte array</returns>
        public static T FromBase62<T>(this string base62, bool inverted = false)
        {
            var array = base62.FromBase62(inverted);

            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.String:
                    return (T)Convert.ChangeType(Encoding.UTF8.GetString(array), typeof(T), CultureInfo.InvariantCulture);
                case TypeCode.Int16:
                    if (BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(array);
                    }
  
                    var list16 = new List<byte>();
                    for(var i = 0; i < 2; i++)
                    {
                        if (array.Length > i)
                        {
                            list16.Add(array[i]);
                            continue;
                        }
                        list16.Add(0);
                    }
                    return (T)Convert.ChangeType(BitConverter.ToInt16([.. list16], 0), typeof(T), CultureInfo.InvariantCulture);
                case TypeCode.Int32:
                    if (BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(array);
                    }

                    var list32 = new List<byte>();
                    for (var i = 0; i < 4; i++)
                    {
                        if (array.Length > i)
                        {
                            list32.Add(array[i]);
                            continue;
                        }
                        list32.Add(0);
                    }

                    return (T)Convert.ChangeType(BitConverter.ToInt32([.. list32], 0), typeof(T), CultureInfo.InvariantCulture);
                case TypeCode.Int64:
                    if (BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(array);
                    }
                    var list64 = new List<byte>();
                    for (var i = 0; i < 8; i++)
                    {
                        if (array.Length > i)
                        {
                            list64.Add(array[i]);
                            continue;
                        }
                        list64.Add(0);
                    }
                    return (T)Convert.ChangeType(BitConverter.ToInt64([.. list64], 0), typeof(T), CultureInfo.InvariantCulture);
                default:
                    throw new Exception($"Type of {typeof(T)} does not support.");
            }
        }

        /// <summary>
        /// Decode a base62-encoded string
        /// </summary>
        /// <param name="base62">Base62 string</param>
        /// <param name="inverted">Use inverted character set</param>
        /// <returns>Byte array</returns>
        public static byte[] FromBase62(this string base62, bool inverted = false)
        {
            if (string.IsNullOrWhiteSpace(base62))
            {
                throw new ArgumentNullException(nameof(base62));
            }

            var characterSet = inverted ? InvertedCharacterSet : DefaultCharacterSet;
            var arr = Array.ConvertAll(base62.ToCharArray(), characterSet.IndexOf);

            var converted = BaseConvert(arr, 62, 256);
            return Array.ConvertAll(converted, Convert.ToByte);
        }

        private static int[] BaseConvert(int[] source, int sourceBase, int targetBase)
        {
            var result = new List<int>();
            var leadingZeroCount = Math.Min(source.TakeWhile(x => x == 0).Count(), source.Length - 1);
            int count;
            while ((count = source.Length) > 0)
            {
                var quotient = new List<int>();
                var remainder = 0;
                for (var i = 0; i != count; i++)
                {
                    var accumulator = source[i] + remainder * sourceBase;
                    var digit = accumulator / targetBase;
                    remainder = accumulator % targetBase;
                    if (quotient.Count > 0 || digit > 0)
                    {
                        quotient.Add(digit);
                    }
                }
                result.Insert(0, remainder);
                source = quotient.ToArray();
            }
            result.InsertRange(0, Enumerable.Repeat(0, leadingZeroCount));
            return result.ToArray();
        }
    }
}
