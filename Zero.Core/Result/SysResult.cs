using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zero.Core.Result
{
    /// <summary>
    /// 返回结果集
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SysResult<T>
    {
        /// <summary>
        /// 错误码
        /// </summary>
        public ErrorCode ErrorCode;

        /// <summary>
        /// 返回结果
        /// </summary>
        public T Result { get; set; }

        /// <summary>
        /// 附加消息
        /// </summary>
        public string Append { get; set; }

        /// <summary>
        /// 是否异常
        /// </summary>
        public bool OccurError
        {
            get { return ErrorCode != ErrorCode.sys_success; }
        }

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success
        {
            get { return ErrorCode == ErrorCode.sys_success; }
        }
    }
}
