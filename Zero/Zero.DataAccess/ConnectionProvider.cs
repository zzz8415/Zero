using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Zero.DataAccess
{
    /// <summary>
    /// 基础数据库配置信息类文件
    /// </summary>
    public class ConnectionProvider : ConfigurationSection
    {
        /// <summary>
        /// 连接信息字典
        /// </summary>
        public Dictionary<int, ConnectInfo> ConnectInfos { get; set; }

        /// <summary>
        /// 转换的类名
        /// </summary>
        public string ChangeToBackClassName { get; set; }

        /// <summary>
        /// 最大异常数
        /// </summary>
        public string SqlExceptionMaxNum { get; set; }

        /// <summary>
        /// 异常状态时间
        /// </summary>
        public string SqlExceptionStatSecond { get; set; }
    }

    /// <summary>
    /// 连接信息
    /// </summary>
    public class ConnectInfo
    {

        /// <summary>
        /// 读取数据库连接串
        /// </summary>
        public string ReadConnectionString { get; set; }

        /// <summary>
        /// 写入数据库连接串
        /// </summary>
        public string WriteConnectionString { get; set; }

        /// <summary>
        /// 备用数据库连接串
        /// </summary>
        public string BackupReadConnectionString { get; set; }
    }
}
