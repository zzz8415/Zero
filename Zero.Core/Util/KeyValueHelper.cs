using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zero.Core.Util
{
    /// <summary>
    /// 
    /// </summary>
    public class KeyValueHelper
    {
        /// <summary>
        /// 组合字符串格式（例：1|共享,2|免费,3|试用）
        /// </summary>
        /// <param name="valueKeys"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetValue(string valueKeys, string key)
        {
            if (string.IsNullOrEmpty(valueKeys) || string.IsNullOrEmpty(key))
            {
                return valueKeys;
            }

            string[] arrItems = valueKeys.Split(',');
            foreach (string item in arrItems)
            {
                string[] arrKeyVals = item.Split('|');
                if (arrKeyVals.Length == 2 && arrKeyVals[1].ToLower().Equals(key.ToLower()))
                {
                    return arrKeyVals[0];
                }
            }

            return "";
        }

        /// <summary>
        /// 组合字符串格式（例：1|共享,2|免费,3|试用）
        /// </summary>
        /// <param name="valueKeys"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetKey(string valueKeys, string value)
        {
            if (string.IsNullOrEmpty(valueKeys) || string.IsNullOrEmpty(value))
            {
                return valueKeys;
            }

            string[] arrItems = valueKeys.Split(',');
            foreach (string item in arrItems)
            {
                string[] arrKeyVals = item.Split('|');
                if (arrKeyVals.Length == 2 && arrKeyVals[0].ToLower().Equals(value.ToLower()))
                {
                    return arrKeyVals[1];
                }
            }

            return string.Empty;
        }
    }
}
