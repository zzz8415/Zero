using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zero.Core.Extensions;
using Zero.Core.Util;
using StackExchange.Redis;
using System.Configuration;
namespace Zero.Redis
{
    /// <summary>
    /// Redis管理
    /// </summary>
    public class RedisManager
    {
        /// <summary>
        /// redis配置文件信息
        /// </summary>
        private static RedisSection redisConfigInfo = new RedisSection();

        private static ConnectionMultiplexer connection = null;

        /// <summary>
        /// 配置
        /// </summary>
        protected static ConnectionMultiplexer Connection
        {
            get
            {
                if (connection == null)
                {
                    var section = ConfigurationManager.GetSection("Redis") as RedisSection;

                    var config = new ConfigurationOptions
                    {
                        Password = section.Password,
                        KeepAlive = section.KeepAlive,
                        ConnectRetry = section.ConnectRetry,
                        AbortOnConnectFail = false,
                        ConnectTimeout = section.ConnectTimeout,
                        DefaultDatabase = section.DBRegion
                    };
                    for (var i = 0; i < section.RedisHosts.Count; i++)
                    {
                        var host = section.RedisHosts[i];
                        config.EndPoints.Add(host.Host, host.Port);
                    }
                    connection = ConnectionMultiplexer.Connect(config);
                }
                return connection;
            }
        }
        /// <summary>
        /// 实例化一个Redis数据库
        /// </summary>
        public static IDatabase GetClient()
        {
            return Connection.GetDatabase();
        }

    }
}
