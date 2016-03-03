using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Zero.DataAccess
{
    /// <summary>
    /// 连接授权接口
    /// </summary>
    public interface IConnectionProvider
    {
        /// <summary>
        /// 获取读数据库授权连接串
        /// </summary>
        /// <returns></returns>
        string GetReadConnectionString();

        /// <summary>
        /// 获取写数据库授权连接串
        /// </summary>
        /// <returns></returns>
        string GetWriteConnectionString();
    }
}
