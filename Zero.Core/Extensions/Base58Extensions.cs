using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Zero.Core.Extensions
{
    /// <summary>
    /// 标准 Base58 + Base58Check 扩展 (支持 Bitcoin / Tron 地址格式)
    /// </summary>
    public static class Base58Extensions
    {
        // Bitcoin / Tron Base58 字符集
        private const string Base58Chars = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";

        // -------------------------
        //      Base58 编码
        // -------------------------

        public static string ToBase58(this byte[] data)
        {
            if (data == null || data.Length == 0) return string.Empty;

            var arr = Array.ConvertAll(data, b => (int)b);
            var base58 = BaseConvert(arr, 256, 58);

            var sb = new StringBuilder();
            foreach (var index in base58)
                sb.Append(Base58Chars[index]);

            return sb.ToString();
        }

        // -------------------------
        //      Base58 解码
        // -------------------------

        public static byte[] FromBase58(this string base58)
        {
            if (string.IsNullOrWhiteSpace(base58)) throw new ArgumentNullException(nameof(base58));

            var data = Array.ConvertAll(base58.ToCharArray(), c => Base58Chars.IndexOf(c));
            if (data.Any(x => x < 0)) throw new FormatException("Invalid Base58 character detected.");

            var base256 = BaseConvert(data, 58, 256);
            return Array.ConvertAll(base256, Convert.ToByte);
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

            var payload = data[..^4]; // 除去末尾校验和
            var checksum = data[^4..];

            if (!payload.Checksum().SequenceEqual(checksum))
                throw new FormatException("Invalid checksum in Base58Check encoding.");

            return payload;
        }

        // -------------------------
        //      TRON 转换工具
        // -------------------------

        public static string ToTronAddress(this byte[] raw20bytes)
        {
            if (raw20bytes is null || raw20bytes.Length != 20) throw new ArgumentException("Raw address must be 20 bytes");

            // TRON 前缀：0x41
            var withPrefix = new byte[] { 0x41 }.Concat(raw20bytes).ToArray();
            return withPrefix.ToBase58Check();
        }

        public static byte[] TronAddressToRaw(this string tronAddress)
        {
            var raw = tronAddress.FromBase58Check();
            if (raw[0] != 0x41) throw new FormatException("Invalid Tron address prefix.");
            return raw[1..]; // 去掉前 1 字节（0x41）
        }

        // -------------------------
        //  ETH (Hex) 和 TRON 互转工具
        // -------------------------

        public static string HexToTronAddress(this string hexAddress)
        {
            if (string.IsNullOrWhiteSpace(hexAddress))
                throw new ArgumentNullException(nameof(hexAddress));

            if (!hexAddress.StartsWith("0x") || hexAddress.Length != 42)
                throw new FormatException("Invalid ETH Hex address format.");

            var raw = HexToBytes(hexAddress[2..]); // 去掉 0x 前缀
            return raw.ToTronAddress();
        }

        public static string TronToHexAddress(this string tronAddress)
        {
            var raw = tronAddress.TronAddressToRaw();

            return "0x" + BitConverter.ToString(raw).Replace("-", "").ToLowerInvariant();
        }

        // -------------------------
        //  Helper 工具函数
        // -------------------------

        private static int[] BaseConvert(int[] digits, int fromBase, int toBase)
        {
            var result = new List<int>();
            var leadingZeros = digits.TakeWhile(x => x == 0).Count();

            while (digits.Length > 0)
            {
                var quotient = new List<int>();
                int remainder = 0;

                foreach (var digit in digits)
                {
                    var value = digit + remainder * fromBase;
                    var q = value / toBase;
                    remainder = value % toBase;

                    if (quotient.Count > 0 || q != 0)
                        quotient.Add(q);
                }

                result.Insert(0, remainder);
                digits = quotient.ToArray();
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

        private static byte[] HexToBytes(string hex)
        {
            return Enumerable.Range(0, hex.Length / 2)
                .Select(i => Convert.ToByte(hex.Substring(i * 2, 2), 16))
                .ToArray();
        }
    }
}
