using System;
using System.Linq;
using System.Text;

using Zero.Core.Extensions;

namespace Zero.Core.Util
{
    /// <summary>
    /// 网络授权帮助类（支持常见基础类型属性）
    /// </summary>
    public class TokenHelper
    {
        /// <summary>
        /// 序列化
        /// </summary>
        public static string Serialize<T>(T model, string encryKey = null) where T : new()
        {
            var type = typeof(T);
            var sb = new StringBuilder();
            var properties = type.GetProperties();

            for (var i = 0; i < properties.Length;)
            {
                var p = properties[i];
                var value = p.GetValue(model, null);
                sb.Append(value?.ToString() ?? string.Empty);
                if (++i < properties.Length)
                {
                    sb.Append('&');
                }
            }
            return encryKey.IsNullOrEmpty()
                ? CryptoHelper.DES_Encrypt(sb.ToString())
                : CryptoHelper.DES_Encrypt(sb.ToString(), encryKey);
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        public static T Deserialize<T>(string token, string encryKey = null) where T : new()
        {
            if (token.IsNullOrEmpty())
                return default;

            string sessionText;
            sessionText = encryKey.IsNullOrEmpty()
                ? CryptoHelper.DES_Decrypt(token)
                : CryptoHelper.DES_Decrypt(token, encryKey);

            var tokens = sessionText.Split('&');
            var type = typeof(T);
            var ps = type.GetProperties();

            if (tokens.Length != ps.Length)
                return default;

            var model = new T();
            for (var i = 0; i < ps.Length; i++)
            {
                var t = ps[i].PropertyType;
                var value = tokens[i];
                ps[i].SetValue(model, SwitchPropertyValue(t, value), null);
            }
            return model;
        }

        private static object SwitchPropertyValue(Type type, string value)
        {
            if (type == typeof(short) || type == typeof(Int16))
                return value.ToInt32(0);
            if (type == typeof(int) || type == typeof(Int32))
                return value.ToInt32(0);
            if (type == typeof(long) || type == typeof(Int64))
                return value.ToInt64(0);
            if (type == typeof(bool) || type == typeof(Boolean))
                return value.ToBoolean();
            if (type == typeof(DateTime))
                return value.ToDateTime(DateTime.MinValue);
            if (type == typeof(DateTimeOffset))
                return value.ToDateTimeOffset(DateTimeOffset.MinValue);
            if (type == typeof(Guid))
                return Guid.TryParse(value, out var guid) ? guid : Guid.Empty;
            return value;
        }
    }
}