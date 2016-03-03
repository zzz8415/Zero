using System;
using System.Configuration;
using System.Collections.Specialized;

namespace Zero.Core.Util
{
    /// <summary>
    /// 获取AppSettings中的配置
    /// </summary>
    public class ConfigHelper
    { 
        #region 应用程序配置节点集合属性
        /// <summary>
        /// 应用程序配置节点集合属性
        /// </summary>
        /// <returns></returns>
        public static NameValueCollection SettingsCollection
        {
            get
            {
                return ConfigurationManager.AppSettings;
            }
        }
        #endregion

        #region 获取appSettings节点值
        /// <summary>
        /// 获取appSettings节点值
        /// </summary>
        /// <param name="key">节点名称</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>节点值</returns>
        public static string GetSetting(string key, string defaultValue)
        {
            try
            {
                if (SettingsCollection == null)
                    return defaultValue;
                return SettingsCollection[key] ?? defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }
        /// <summary>
        /// 获取appSettings节点值
        /// </summary>
        /// <param name="key">节点名称</param>
        /// <returns>节点值</returns>
        public static string GetSetting(string key)
        {
            return GetSetting(key, "");
        }

        /// <summary>
        /// 获取appSettings节点值，并转换为bool值
        /// </summary>
        /// <param name="key">节点名称</param>
        /// <returns></returns>
        public static bool GetBoolean(string key)
        {
            string tmp = GetSetting(key);
            return tmp.Equals("true", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 获取appSettings节点值，并转换为int值
        /// </summary>
        /// <param name="key">节点名称</param>
        /// <param name="defaultValue">节点不存在或不是数值时的默认值</param>
        /// <returns></returns>
        public static Int32 GetInt32(string key, Int32 defaultValue = 0)
        {
            string tmp = GetSetting(key);
            if (string.IsNullOrEmpty(tmp))
                return defaultValue;
            Int32 ret;
            if (Int32.TryParse(tmp, out ret))
                return ret;
            else
                return defaultValue;
        }

        /// <summary>
        /// 获取appSettings节点值，并转换为long值
        /// </summary>
        /// <param name="key">节点名称</param>
        /// <param name="defaultValue">节点不存在或不是数值时的默认值</param>
        /// <returns></returns>
        public static Int64 GetInt64(string key, Int64 defaultValue = 0)
        {
            string tmp = GetSetting(key);
            if (string.IsNullOrEmpty(tmp))
                return defaultValue;
            Int64 ret;
            if (Int64.TryParse(tmp, out ret))
                return ret;
            else
                return defaultValue;
        }

        /// <summary>
        /// 获取appSettings节点值，并转换为DateTime值
        /// </summary>
        /// <param name="key">节点名称</param>
        /// <param name="defaultValue">节点不存在或不是时间时的默认值</param>
        /// <returns></returns>
        public static DateTime GetDateTime(string key, DateTime? defaultValue = null)
        {
            string tmp = GetSetting(key);
            if (!string.IsNullOrEmpty(tmp))
            {
                DateTime ret;
                if (DateTime.TryParse(tmp, out ret))
                    return ret;
            }
            if (defaultValue == null)
                return DateTime.MinValue;
            else
                return defaultValue.Value;
        }

        /// <summary>
        /// 获取加密了的appSettings节点值
        /// </summary>
        /// <param name="key">节点名称</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>节点值</returns>
        public static string GetEncSetting(string key, string defaultValue = null)
        {
            string ret = GetSetting(key, defaultValue);
            if (string.IsNullOrEmpty(ret) || ret == defaultValue)
                return ret;
            return CryptoHelper.DES_Decrypt(ret);
            //return GetConnectionString(key, Key);
        }
        #endregion

        #region 获取指定名称的连接字符串
        /// <summary>
        /// 获取指定名称的连接字符串
        /// </summary>
        /// <param name="connName">连接串节点名称</param>
        /// <param name="mKey">加密Key</param>
        /// <returns>解密后的连接串</returns>
        public static string GetConnectionString(string connName, string mKey = null)
        {
            try
            {
                string conn = GetSetting(connName);
                if (mKey != null)
                {
                    conn = CryptoHelper.DES_Decrypt(conn, mKey);
                }
                else
                {
                    conn = CryptoHelper.DES_Decrypt(conn);
                }

                return conn;
            }
            catch
            {
                return string.Empty;
            }
        }
        #endregion
    }
}
