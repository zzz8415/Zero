using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using Zero.Core.Web;

namespace Zero.Core.Util
{
    /// <summary>
    /// 自定义配置
    /// </summary>
    public class CustomHelper
    {

        private static string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Custom.Config");

        /// <summary>
        /// 设置读取的配置文件路径 
        /// </summary>
        /// <param name="filePath"></param>
        public static void Redirect(string filePath)
        {
            CustomHelper.filePath = filePath;
        }

        private static Configuration Config
        {
            get
            {
                return GetConfig();
            }
        }

        private static KeyValueConfigurationCollection Collection
        {
            get
            {
                return Config.AppSettings.Settings;
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        private static Configuration GetConfig()
        {
            var key = CacheHelper.RenderKey(filePath);
            return CacheHelper.GetByFileDependency(key, filePath, () =>
            {
                var map = new ExeConfigurationFileMap
                {
                    ExeConfigFilename = filePath
                };
                return ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
            });
        }

        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetValue(string key)
        {
            return GetValue(key, string.Empty);
        }

        /// <summary>
        /// 获取值,如果为空则返回默认值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string GetValue(string key, string defaultValue)
        {
            return Collection == null ? defaultValue : Collection[key]?.Value ?? defaultValue;
        }

        /// <summary>
        /// 设置单值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void SetValue(string key, string value)
        {
            if (Collection != null)
            {
                Collection[key].Value = value;
                Config.Save(ConfigurationSaveMode.Minimal);
            }
        }

        /// <summary>
        /// 设置值集
        /// </summary>
        /// <param name="values"></param>
        public static void SetValue(Dictionary<string, string> values)
        {
            if (Collection != null)
            {
                foreach (var value in values)
                {
                    Collection[value.Key].Value = value.Value;
                }
                Config.Save(ConfigurationSaveMode.Minimal);
            }
        }
    }
}
