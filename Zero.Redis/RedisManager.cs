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
        private static ConfigurationOptions options = null;

        private static object _locker = new object();

        /// <summary>
        /// redis配置
        /// </summary>
        protected static ConfigurationOptions Options
        {
            get
            {
                if (options == null)
                {
                    lock (_locker)
                    {
                        if (options == null)
                        {
                            var section = ConfigurationManager.GetSection("Redis") as RedisSection;

                            options = new ConfigurationOptions
                            {
                                Password = section.Password,
                                KeepAlive = section.KeepAlive,
                                ConnectRetry = section.ConnectRetry,
                                AbortOnConnectFail = false,
                                ConnectTimeout = section.ConnectTimeout,
                                DefaultDatabase = section.DBRegion,
                                SyncTimeout = section.SyncTimeout
                            };
                            for (var i = 0; i < section.RedisHosts.Count; i++)
                            {
                                var host = section.RedisHosts[i];
                                options.EndPoints.Add(host.Host, host.Port);
                            }
                        }
                    }
                   
                }
                return options;
            }
        }

        /// <summary>
        /// 创建Redis连接
        /// </summary>
        /// <returns></returns>
        public static ConnectionMultiplexer GetConnect()
        {
            return ConnectionMultiplexer.Connect(Options);
        }

    }
}
