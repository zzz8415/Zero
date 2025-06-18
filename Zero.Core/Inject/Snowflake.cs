using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using System;
using System.Net;
using System.Threading;

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
        private const int WorkerIdBits = 5;
        private const int SequenceBits = 12;
        private const long MaxWorkerId = -1L ^ (-1L << WorkerIdBits);
        private const long MaxSequence = -1L ^ (-1L << SequenceBits);

        private long _lastTimestamp = -1L;
        private long _sequence = 0L;
        private long _workerId;
        private long _offsetTicks = 0;

        public SnowflakeConfig Config { get; }

        public Snowflake(WebConfig webConfig)
        {
            Config = webConfig.Get<SnowflakeConfig>("Snowflake") ?? new SnowflakeConfig
            {
                WorkId = 0,
                OffsetDate = new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero)
            };

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

            if (Config.WorkId < 0 || Config.WorkId > MaxWorkerId)
            {
                // logger.Warn($"Invalid WorkId {Config.WorkId}, using 0 instead");
                Config.WorkId = 0;
            }

            _workerId = Config.WorkId;
            _offsetTicks = Config.OffsetDate.ToUnixTimeMilliseconds();
        }

        public long NewID()
        {
            while (true)
            {
                var timestamp = GetCurrentTimestamp();
                var lastTimestamp = Interlocked.Read(ref _lastTimestamp);

                // 处理时钟回拨：等待时钟恢复
                if (timestamp < lastTimestamp)
                {
                    var spinWait = new SpinWait();
                    while (timestamp < lastTimestamp)
                    {
                        spinWait.SpinOnce();
                        timestamp = GetCurrentTimestamp();
                    }
                }

                long sequence;
                if (timestamp == lastTimestamp)
                {
                    sequence = (Interlocked.Increment(ref _sequence)) & MaxSequence;
                    if (sequence == 0)
                    {
                        timestamp = WaitNextMillis(lastTimestamp);
                        if (Interlocked.CompareExchange(ref _lastTimestamp, timestamp, lastTimestamp) == lastTimestamp)
                        {
                            Interlocked.Exchange(ref _sequence, 0);
                        }
                        continue;
                    }
                }
                else
                {
                    sequence = 0;
                    if (Interlocked.CompareExchange(ref _lastTimestamp, timestamp, lastTimestamp) != lastTimestamp)
                    {
                        continue;
                    }
                    Interlocked.Exchange(ref _sequence, sequence);
                }

                return ((timestamp - _offsetTicks) << (WorkerIdBits + SequenceBits))
                     | (_workerId << SequenceBits)
                     | sequence;
            }
        }

        private long GetCurrentTimestamp()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        private long WaitNextMillis(long lastTimestamp)
        {
            var spinWait = new SpinWait();
            var timestamp = GetCurrentTimestamp();
            while (timestamp <= lastTimestamp)
            {
                spinWait.SpinOnce();
                timestamp = GetCurrentTimestamp();
            }
            return timestamp;
        }
    }
}
