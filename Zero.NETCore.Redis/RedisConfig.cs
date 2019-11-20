using System;
using System.Collections.Generic;
using System.Text;

namespace Zero.NETCore.Redis
{
    /// <summary>
    /// Redis配置
    /// </summary>
    public class RedisConfig
    {
        /// <summary>
        /// Host
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// 端口
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 长连接秒数
        /// </summary>
        public int KeepAlive { get; set; } = 30;

        /// <summary>
        /// 重试次数
        /// </summary>
        public int ConnectRetry { get; set; } = 3;

        /// <summary>
        /// 连接超时毫秒数
        /// </summary>
        public int ConnectTimeout { get; set; } = 3000;

        /// <summary>
        /// 异步响应超时毫秒数
        /// </summary>
        public int SyncTimeout { get; set; } = 3000;

        /// <summary>
        /// 数据分片
        /// </summary>
        public int DBRegion { get; set; } = 0;
    }
}
