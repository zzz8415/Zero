using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Zero.NETCore.Util
{
    /// <summary>
    /// 枚举帮助类
    /// </summary>
    public class EnumHelper
    {
        /// <summary>
        /// 从枚举类型返回枚举值和描述信息的组合字符串（如：1|共享,2|免费,3|试用）
        /// </summary>
        /// <param name="enumType">Type,该参数的格式为typeof(需要读的枚举类型)</param>
        /// <returns>键值对</returns>
        public static string GetNVCFromEnumValue(Type enumType)
        {
            StringBuilder sb = new StringBuilder();
            string strDesc = string.Empty;
            Type typeDescription = typeof(DescriptionAttribute);
            System.Reflection.FieldInfo[] fields = enumType.GetFields();
            foreach (FieldInfo field in fields)
            {
                if (field.FieldType.IsEnum)
                {
                    object[] arr = field.GetCustomAttributes(typeDescription, true);
                    if (arr.Length > 0)
                    {
                        DescriptionAttribute aa = (DescriptionAttribute)arr[0];
                        strDesc = aa.Description;
                    }
                    else
                    {
                        strDesc = field.Name;
                    }

                    sb.AppendFormat("{0}|{1},", (int)enumType.InvokeMember(field.Name, BindingFlags.GetField, null, null, null), strDesc);
                }
            }
            return sb.ToString().TrimEnd(',');
        }

    }
}
