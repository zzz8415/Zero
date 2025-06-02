using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using System;
using System.Net;

using Zero.Core.Attribute;
using Zero.Core.Config;
using Zero.Core.Extensions;

namespace Zero.Core.Inject
{
    /// <summary>
    /// 雪花算法 配置如下
    /// <Snowflake>
    ///     <WorkID>0</WorkID>
    ///     <OffsetDate>2010-10-10</OffsetDate>
    /// </Snowflake>
    /// </summary>
    [Inject(OptionsLifetime = ServiceLifetime.Singleton)]
    public class Snowflake
    {
        private long _lastTicks = 0L;
        private long _lastFlowID = 0L;
        private static readonly object _lock = new();
        //public long _workID = 0;
        private readonly long _maxFlowID = 1L << 4;
        private readonly long _maxWorkID = 1L << 6;
        private readonly long _offsetTicks = 0;

        /// <summary>
        /// 配置
        /// </summary>
        public SnowflakeConfig Config { get; set; }

        /// <summary>
        /// DI容器注册
        /// </summary>
        /// <param name="configuration"></param>
        public Snowflake(WebConfig webConfig)
        {
            Config = webConfig.Get<SnowflakeConfig>("Snowflake");
            if (Config == null)
            {
                Config = new SnowflakeConfig
                {
                    WorkId = 0,
                    OffsetDate = new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero)
                };
                return;
            }
            if (Config.HostNameList?.Count > 0)
            {
                var hostName = Dns.GetHostName();
                if (!hostName.IsNullOrEmpty())
                {
                    var index = Config.HostNameList.IndexOf(hostName);
                    if (index >= 0)
                    {
                        Config.WorkId = index;
                    }
                }
            }
            if(Config.WorkId < 0 || Config.WorkId > _maxWorkID)
            {
                Config.WorkId = 0;
            }

            _offsetTicks = Config.OffsetDate.Ticks;
        }

        /// <summary>
        /// 生成新的ID
        /// </summary>
        /// <returns></returns>
        public long NewID()
        {
            lock (_lock)
            {
                long ticks = GetTicks();
                ResetFlowID(ticks);
                long flowID = GetFlowID();
                // 如果流水号溢出,重新获取时间戳
                if (flowID >= _maxFlowID)
                {
                    ticks = GetNextTicks();
                    ResetFlowID(ticks);
                    flowID = GetFlowID();
                }
                return ticks | GetWorkID() | flowID;
            }
        }

        /// <summary>
        /// 重置流水号
        /// </summary>
        /// <param name="ticks"></param>
        private void ResetFlowID(long ticks)
        {
            if (ticks > _lastTicks)
            {
                _lastTicks = ticks;
                _lastFlowID = 0;
            }
        }

        /// <summary>
        /// 获取时间戳(55位)
        /// </summary>
        /// <returns></returns>
        private long GetTicks()
        {
            return ((DateTimeOffset.UtcNow.Ticks - _offsetTicks) << 8) & long.MaxValue;
        }

        /// <summary>
        /// 流水号溢出获取下一个时间戳
        /// </summary>
        /// <returns></returns>
        private long GetNextTicks()
        {
            long ticks;
            do
            {
                ticks = GetTicks();
            }
            while (ticks == _lastTicks);
            return ticks;
        }

        /// <summary>
        /// 获取机器ID(5位)
        /// </summary>
        /// <returns></returns>
        private long GetWorkID()
        {
            return Config.WorkId << 3;
        }

        /// <summary>
        /// 获取流水号(3位)
        /// </summary>
        /// <returns></returns>
        private long GetFlowID()
        {
            return _lastFlowID++;
        }
    }
}
