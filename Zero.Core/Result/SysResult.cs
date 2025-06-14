using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;

namespace Zero.Core.Result
{

    /// <summary>
    /// 返回结果集
    /// </summary>
    public class SysResult
    {
        /// <summary>
        /// 错误码
        /// </summary>
        public ErrorCode Code { get; set; }

        /// <summary>
        /// 附加消息
        /// </summary>
        #nullable enable
        public string? ErrorDesc { get; set; }

        /// <summary>
        /// 是否异常
        /// </summary>
        [JsonIgnore]
        public bool OccurError
        {
            get { return Code != ErrorCode.SYS_SUCCESS; }
        }

        /// <summary>
        /// 是否成功
        /// </summary>
        [JsonIgnore]
        public bool Success
        {
            get { return Code == ErrorCode.SYS_SUCCESS; }
        }

    }


    /// <summary>
    /// 返回结果集
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SysResult<T> : SysResult
    {

        /// <summary>
        /// 返回结果
        /// </summary>
        #nullable enable
        public T? Result { get; set; }

    }

    public class SysResult<T1, T2> : SysResult
    {

        /// <summary>
        /// 返回结果
        /// </summary>
        public T1? Result { get; set; }

        /// <summary>
        /// 扩展数据
        /// </summary>
        public T2? Extend { get; set; }

    }

}
