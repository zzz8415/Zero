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
    public class RedisServerSection : ConfigurationSection
    {
        /// <summary>
        /// Redis服务器地址
        /// </summary>
        [ConfigurationProperty("host", IsRequired = true)]
        public string Host
        {
            get { return this["host"].ToString(); }
            set { this["host"] = value; }
        }

        /// <summary>
        /// Redis服务器端口
        /// </summary>
        [ConfigurationProperty("port", IsRequired = true)]
        public int Port
        {
            get { return this["port"].ToInt32(0); }
            set { this["port"] = value; }
        }

    }

}
