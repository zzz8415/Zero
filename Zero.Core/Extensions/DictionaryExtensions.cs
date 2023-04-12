using System;
using System.Collections.Generic;
using System.Text;

namespace Zero.Core.Extensions
{
    /// <summary>
    /// 字典扩展
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        /// 获取值
        /// </summary>
        /// <typeparam name="Tkey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="source"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static TValue GetValue<Tkey, TValue>(this Dictionary<Tkey, TValue> source, Tkey key)
        {
            TValue model = default;
            if (source != null && key != null)
            {
                source.TryGetValue(key, out model);
            }
            return model;
        }
    }
}
