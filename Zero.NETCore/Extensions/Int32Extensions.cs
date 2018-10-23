using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zero.NETCore.Extensions
{
    /// <summary>
    /// int扩展
    /// </summary>
    public static class Int32Extensions
    {
        /// <summary>
        /// 将字符串转成指定的枚举类型(字符串可以是枚举的名称也可以是枚举值)
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <param name="source">源字符串</param>
        /// <param name="defaultValue">如果转换失败，返回默认的枚举项</param>
        /// <returns></returns>
        public static T ToEnum<T>(this int source, T defaultValue)
        {
            try
            {
                T value = (T)Enum.Parse(typeof(T), source.ToString(), true);
                if (Enum.IsDefined(typeof(T), value))
                {
                    return value;
                }
            }
            catch { }
            return defaultValue;
        }

        /// <summary>
        /// 将字符串转成指定的枚举类型(字符串可以是枚举的名称也可以是枚举值)
        /// <remarks>支持枚举值的并集</remarks>
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <param name="source">源字符串</param>
        /// <param name="defaultValue">如果转换失败，返回默认的枚举项</param>
        /// <returns></returns>
        public static T ToEnumExt<T>(this int source, T defaultValue)
        {
            try
            {
                return (T)Enum.Parse(typeof(T), source.ToString(), true);
            }
            catch { }
            return defaultValue;
        }
    }
}
