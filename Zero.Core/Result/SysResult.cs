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
        public ErrorCode Code;

        /// <summary>
        /// 附加消息
        /// </summary>
        public string ErrorDesc { get; set; }

        /// <summary>
        /// 是否异常
        /// </summary>
        [JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public bool OccurError
        {
            get { return Code != ErrorCode.sys_success; }
        }

        /// <summary>
        /// 是否成功
        /// </summary>
        [JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public bool Success
        {
            get { return Code == ErrorCode.sys_success; }
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
        public T Result { get; set; }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <returns></returns>
        public object GetResult()
        {
            return this.Result;
        }

    }

}
