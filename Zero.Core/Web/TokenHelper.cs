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
    /// <typeparam name="T"></typeparam>
    public class TokenHelper
    {
        /// <summary>
        /// 序列化
        /// </summary>
        /// <returns></returns>
        public static string Serialize<T>(T model) where T : new()
        {
            var type = typeof(T);

            StringBuilder sb = new StringBuilder();

            foreach (var p in type.GetProperties())
            {
                sb.Append(model.GetType().GetProperty(p.Name).GetValue(model, null).ToString() + "&");
            }

            return CryptoHelper.DES_Encrypt(sb.ToString().TrimEnd('&'));
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static T Deserialize<T>(string token) where T : new()
        {
            if (token.IsNullOrEmpty())
                return default(T);
            var sessionText = CryptoHelper.DES_Decrypt(token);
            var tokens = sessionText.Split('&');

            var type = typeof(T);

            var ps = type.GetProperties();

            if (tokens.Length != ps.Length)
                return default(T);

            var model = new T();

            for(var i = 0; i < ps.Length; i++)
            {
                ps[i].SetValue(model, tokens[i], null);
            }

            return model;
        }
    }
}
