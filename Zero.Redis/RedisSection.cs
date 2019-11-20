using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Zero.Core.Util;
using Zero.Core.Extensions;
namespace Zero.Redis
{
    /// <summary>
    /// Redis配置信息类文件
    /// </summary>
    public class RedisSection : ConfigurationSection
    {
        /// <summary>
        /// Redis Host
        /// </summary>
        [ConfigurationProperty("servers")]
        public RedisHostCollection RedisHosts
        {
            get { return (RedisHostCollection)this["servers"]; }
        }
          
        /// <summary>
        /// 密码
        /// </summary>
        [ConfigurationProperty("password")]
        public string Password
        {
            get { return this["password"].ToString(); }
            set { this["password"] = value; }
        }

        /// <summary>
        /// 长连接秒数
        /// </summary>
        [ConfigurationProperty("keepAlive", DefaultValue = 30)]
        public int KeepAlive
        {
            get { return this["keepAlive"].ToInt32(30); }
            set { this["keepAlive"] = value; }
        }

        /// <summary>
        /// 重试次数
        /// </summary>
        [ConfigurationProperty("connectRetry", DefaultValue=3)]
        public int ConnectRetry
        {
            get { return this["connectRetry"].ToInt32(3); }
            set { this["connectRetry"] = value; }
        }

        /// <summary>
        /// 连接超时毫秒数
        /// </summary>
        [ConfigurationProperty("connectTimeout", DefaultValue = 3000)]
        public int ConnectTimeout
        {
            get { return this["connectTimeout"].ToInt32(3000); }
            set { this["connectTimeout"] = value; }
        }

        /// <summary>
        /// 重试次数
        /// </summary>
        [ConfigurationProperty("dbRegion", DefaultValue = 0)]
        public int DBRegion
        {
            get { return this["dbRegion"].ToInt32(0); }
            set { this["dbRegion"] = value; }
        }

        /// <summary>
        /// 响应超时毫秒数
        /// </summary>
        [ConfigurationProperty("syncTimeout", DefaultValue = 3000)]
        public int SyncTimeout
        {
            get { return this["syncTimeout"].ToInt32(3000); }
            set { this["syncTimeout"] = value; }
        }
    }

}
