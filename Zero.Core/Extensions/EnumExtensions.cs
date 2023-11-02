using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Zero.Core.Extensions
{
    /// <summary>
    /// 枚举扩展方法
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// 将枚举转换为数字
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static int ToInt32(this Enum source)
        {
            return Convert.ToInt32(source);
        }

        private static Dictionary<Type, Dictionary<string, string>> _enumDiscriptionArray = new();

        /// <summary>
        /// 获取枚举值的描述信息
        /// </summary>
        /// <param name="source">枚举值</param>
        /// <returns></returns>
        public static string GetDescription(this Enum source)
        {
            var type = source.GetType();
            var array = _enumDiscriptionArray.GetValueOrDefault(type);
            if(array == null)
            {
                var typeDescription = typeof(DescriptionAttribute);
                var fields = type.GetFields();
                array = fields.ToDictionary(x => x.Name, x =>
                {
                    object[] arr = x.GetCustomAttributes(typeDescription, true);
                    if (arr.Length > 0)
                    {
                        DescriptionAttribute da = (DescriptionAttribute)arr[0];
                        return da.Description;
                    }
                    else
                    {
                        return x.Name;
                    }
                });
                _enumDiscriptionArray.Add(type, array);
            }

            return array.GetValueOrDefault(source.ToString());
        }

        /// <summary>
        /// 获取枚举值的描述信息
        /// </summary>
        /// <param name="enumType">Type,该参数的格式为typeof(需要读的枚举类型)</param>
        /// <param name="source">枚举值</param>
        /// <returns></returns>
        public static string GetDescription(this Enum source, Type enumType)
        {
            var array = _enumDiscriptionArray.GetValueOrDefault(enumType);
            if (array == null)
            {
                var typeDescription = typeof(DescriptionAttribute);
                var fields = enumType.GetFields();
                array = fields.ToDictionary(x => x.Name, x =>
                {
                    object[] arr = x.GetCustomAttributes(typeDescription, true);
                    if (arr.Length > 0)
                    {
                        DescriptionAttribute da = (DescriptionAttribute)arr[0];
                        return da.Description;
                    }
                    else
                    {
                        return x.Name;
                    }
                });
                _enumDiscriptionArray.Add(enumType, array);
            }

            return array.GetValueOrDefault(source.ToString());
        }
    }
}
