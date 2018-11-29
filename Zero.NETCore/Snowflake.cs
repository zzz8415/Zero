using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Zero.NETCore
{
    public class Snowflake
    {
        private long _lastFlowID = 0L;
        private readonly long _maxFlowID = 1L << 5;
        private readonly static object _lock = new object();
        private long _workID = 0;
        private readonly long _maxWorkID = 1L << 6;

        public Snowflake(long workID = 0)
        {
             _workID = workID >= _maxFlowID ? 0 : workID;
        }

        public Snowflake(IConfiguration configuration)
        {
            var workID = configuration.GetValue("Snowflake.WorkID", 0);
            _workID = workID >= _maxFlowID ? 0 : workID;
        }

        /// <summary>
        /// 生成新的ID
        /// </summary>
        /// <returns></returns>
        public long NewID(long workID = long.MinValue)
        {
            if(workID != long.MinValue || workID >= _maxFlowID)
            {
                _workID = workID;
            }
       
            lock (_lock)
            {
                return GetTicks() | GetWorkID() | GetFlowID();
            }
        }

        public long NextID()
        {
            lock (_lock)
            {
                return GetTicks() | GetWorkID() | GetFlowID();
            }
        }

        /// <summary>
        /// 获取时间戳(53位)
        /// </summary>
        /// <returns></returns>
        private long GetTicks()
        {
            return (DateTime.UtcNow.Ticks << 8) & long.MaxValue;
        }

        /// <summary>
        /// 获取机器ID(5位)
        /// </summary>
        /// <returns></returns>
        private long GetWorkID()
        {
            return _workID << 3;
        }

        /// <summary>
        /// 获取流水号(3位)
        /// </summary>
        /// <returns></returns>
        private long GetFlowID()
        {
            _lastFlowID++;
            if (_lastFlowID >= _maxFlowID)
            {
                _lastFlowID = 0;
            }
            return _lastFlowID;
        }
    }
}
