using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zero.Core.Result
{

    /// <summary>
    /// 返回结果集
    /// </summary>
    public abstract class SysResult
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
        public bool OccurError
        {
            get { return Code != ErrorCode.sys_success; }
        }

        /// <summary>
        /// 是否成功
        /// </summary>
        [JsonIgnore]
        public bool Success
        {
            get { return Code == ErrorCode.sys_success; }
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <returns></returns>
        public abstract object GetData();

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
        public override object GetData()
        {
            return this.Result;
        }

    }

}
