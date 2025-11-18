using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Zero.Core.Extensions
{
    /// <summary>
    /// Base58 + Base58Check 扩展 (支持 Bitcoin / Tron 地址格式)
    /// </summary>
    public static class Base58Extensions
    {
        // Bitcoin / Tron 字符集 (无 0, O, I, l)
        private const string Base58Chars = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";

        // -------------------------
        //         Base58 编码
        // -------------------------

        public static string ToBase58(this byte[] data)
        {
            if (data == null || data.Length == 0) return string.Empty;

            var arr = Array.ConvertAll(data, b => (int)b);
            var base58 = BaseConvert(arr, 256, 58);

            var sb = new StringBuilder();
            foreach (var index in base58)
            {
                sb.Append(Base58Chars[index]);
            }
            return sb.ToString();
        }

        public static string ToBase58(this string text) => Encoding.UTF8.GetBytes(text).ToBase58();

        public static string ToBase58(this short val) => BitConverter.GetBytes(val).ToBase58Formatted();
        public static string ToBase58(this int val) => BitConverter.GetBytes(val).ToBase58Formatted();
        public static string ToBase58(this long val) => BitConverter.GetBytes(val).ToBase58Formatted();

        // 格式化处理（去除尾 0、转换为大端序）
        private static string ToBase58Formatted(this byte[] data)
        {
            var cleaned = data.TrimTrailingZeros().EnsureBigEndian();
            return cleaned.ToBase58();
        }

        // -------------------------
        //         Base58 解码
        // -------------------------

        public static byte[] FromBase58(this string base58)
        {
            if (string.IsNullOrWhiteSpace(base58)) throw new ArgumentNullException(nameof(base58));

            var data = Array.ConvertAll(base58.ToCharArray(), Base58Chars.IndexOf);

            var base256 = BaseConvert(data, 58, 256);
            return Array.ConvertAll(base256, Convert.ToByte);
        }

        public static T FromBase58<T>(this string base58)
        {
            var bytes = base58.FromBase58();

            return typeof(T) switch
            {
                var t when t == typeof(string) => (T)Convert.ChangeType(Encoding.UTF8.GetString(bytes), typeof(T)),
                var t when t == typeof(short) => (T)(object)bytes.ToNumber(BitConverter.ToInt16),
                var t when t == typeof(int) => (T)(object)bytes.ToNumber(BitConverter.ToInt32),
                var t when t == typeof(long) => (T)(object)bytes.ToNumber(BitConverter.ToInt64),
                _ => throw new NotSupportedException($"Type {typeof(T)} is not supported.")
            };
        }

        // -------------------------
        //      Base58Check (链地址)
        // -------------------------

        public static string ToBase58Check(this byte[] data)
        {
            var checksum = data.Checksum();
            var combined = data.Concat(checksum).ToArray();
            return combined.ToBase58();
        }

        public static byte[] FromBase58Check(this string base58Check)
        {
            var data = base58Check.FromBase58();
            if (data.Length < 4) throw new FormatException("Invalid Base58Check payload.");

            var payload = data[..^4];
            var checksum = data[^4..];

            if (!payload.Checksum().SequenceEqual(checksum))
                throw new FormatException("Invalid checksum in Base58Check encoding.");

            return payload;
        }

        // -------------------------
        //    TRON 示例 (T 地址)
        // -------------------------

        public static string ToTronAddress(this byte[] rawAddress)
        {
            // TRON 地址前缀为 0x41（十进制 65）
            var addressWithPrefix = new byte[] { 0x41 }.Concat(rawAddress).ToArray();

            return addressWithPrefix.ToBase58Check();
        }

        public static byte[] TronAddressToRaw(this string tronAddress)
        {
            var raw = tronAddress.FromBase58Check();
            if (raw[0] != 0x41) throw new FormatException("Invalid Tron address prefix.");
            return raw[1..];
        }

        // -------------------------
        //     Helper 工具函数
        // -------------------------

        private static int[] BaseConvert(int[] source, int fromBase, int toBase)
        {
            var result = new List<int>();
            var leadingZeros = source.TakeWhile(x => x == 0).Count();

            while (source.Length > 0)
            {
                var quotient = new List<int>();
                int remainder = 0;

                foreach (var digit in source)
                {
                    var value = digit + remainder * fromBase;
                    var q = value / toBase;
                    remainder = value % toBase;

                    if (quotient.Count > 0 || q > 0)
                        quotient.Add(q);
                }

                result.Insert(0, remainder);
                source = quotient.ToArray();
            }

            result.InsertRange(0, Enumerable.Repeat(0, leadingZeros));
            return result.ToArray();
        }

        private static byte[] Checksum(this byte[] data)
        {
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(sha256.ComputeHash(data));
            return hash[..4];
        }

        private static byte[] TrimTrailingZeros(this byte[] data)
        {
            int i = data.Length - 1;
            while (i >= 0 && data[i] == 0) i--;
            return data.Take(i + 1).ToArray();
        }

        private static T ToNumber<T>(this byte[] bytes, Func<byte[], int, T> converter)
        {
            int size = typeof(T) == typeof(short) ? sizeof(short)
                     : typeof(T) == typeof(int) ? sizeof(int)
                     : typeof(T) == typeof(long) ? sizeof(long)
                     : throw new NotSupportedException($"Type {typeof(T)} is not supported.");

            Array.Resize(ref bytes, size);
            if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
            return converter(bytes, 0);
        }

        private static byte[] EnsureBigEndian(this byte[] data)
        {
            if (BitConverter.IsLittleEndian) Array.Reverse(data);
            return data;
        }
    }
}
