using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zero.Core.Util;
using Zero.Core.Extensions;

namespace Zero.Core.Web
{
    /// <summary>
    /// 网络授权帮助类(只支持所有属性为字符串类型的类)
    /// </summary>
    public class TokenHelper
    {
        /// <summary>
        /// 序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <param name="encryKey"></param>
        /// <returns></returns>
        public static string Serialize<T>(T model, string encryKey = null) where T : new()
        {
            var type = typeof(T);

            StringBuilder sb = new StringBuilder();

            var properties = type.GetProperties();

            for (var i = 0; i < properties.Length;)
            {
                var p = properties[i];
                sb.Append(model.GetType().GetProperty(p.Name).GetValue(model, null).ToString());
                if (++i < properties.Length)
                {
                    sb.Append("&");
                }
            }
            if (encryKey.IsNullOrEmpty())
            {
                return CryptoHelper.DES_Encrypt(sb.ToString());
            }
            else
            {
                return CryptoHelper.DES_Encrypt(sb.ToString(), encryKey);
            }
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="token"></param>
        /// <param name="encryKey"></param>
        /// <returns></returns>
        public static T Deserialize<T>(string token, string encryKey = null) where T : new()
        {
            if (token.IsNullOrEmpty())
                return default(T);

            var sessionText = encryKey.IsNullOrEmpty() ?
                CryptoHelper.DES_Decrypt(token) :
                CryptoHelper.DES_Decrypt(token, encryKey);

            var tokens = sessionText.Split('&');

            var type = typeof(T);

            var ps = type.GetProperties();

            if (tokens.Length != ps.Length)
                return default(T);

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
            if (type.Name.Equals("Int16", StringComparison.OrdinalIgnoreCase))
            {
                return Convert.ToInt16(value);
            }
            if (type.Name.Equals("Int32", StringComparison.OrdinalIgnoreCase))
            {
                return value.ToInt32(0);
            }
            if (type.Name.Equals("Int64", StringComparison.OrdinalIgnoreCase))
            {
                return value.ToInt64(0);
            }
            if (type.Name.Equals("Boolean", StringComparison.OrdinalIgnoreCase))
            {
                return value.ToBoolean();
            }
            if (type.Name.Equals("DateTime", StringComparison.OrdinalIgnoreCase))
            {
                return value.ToDateTime(DateTime.MinValue);
            }
            if (type.Name.Equals("Guid", StringComparison.OrdinalIgnoreCase))
            {
                return Guid.Parse(value);
            }

            return value;
        }
    }
}
