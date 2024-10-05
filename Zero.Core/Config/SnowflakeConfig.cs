using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zero.Core.Config
{
    public class SnowflakeConfig
    {
        /// <summary>
        /// 初始化机器码,范围0-31,如果超过范围,则WorkID等于0
        /// </summary>
        public int WorkId { get; set; }

        /// <summary>
        /// 日期偏移量
        /// </summary>
        public DateTime OffsetDate { get; set; }

        /// <summary>
        /// 主机名
        /// </summary>
        public List<string> HostNameList { get; set; }
    }
}
