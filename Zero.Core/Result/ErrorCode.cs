using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Zero.Core.Result
{
    /// <summary>
    /// 错误码
    /// </summary>
    public enum ErrorCode
    {
        /// <summary>
        /// 操作成功
        /// </summary>
        [Description("SUCCESS")]
        SYS_SUCCESS = 0,

        /// <summary>
        /// 服务器异常
        /// </summary>
        [Description("FAIL")]
        SYS_FAIL = 1,

        /// <summary>
        /// 参数值格式有误
        /// </summary>
        [Description("PARAM ERROR")]
        SYS_PARAM_FORMAT_ERROR = 2,

        /// <summary>
        /// 相关资源不存在
        /// </summary>
        [Description("RESOURCE NOT FOUND")]
        SYS_RESOURCE_NOT_FOUND = 3,

        /// <summary>
        /// 预留错误码1
        /// </summary>
        USER_RESERVED_1 = 11,

        /// <summary>
        /// 预留错误码2
        /// </summary>
        USER_RESERVED_2 = 12,

        /// <summary>
        /// 预留错误码3
        /// </summary>
        USER_RESERVED_3 = 13,

        /// <summary>
        /// 预留错误码4
        /// </summary>
        USER_RESERVED_4 = 14,

        /// <summary>
        /// 预留错误码5
        /// </summary>
        USER_RESERVED_5 = 15,

        /// <summary>
        /// 预留错误码6
        /// </summary>
        USER_RESERVED_6 = 16,

        /// <summary>
        /// 预留错误码7
        /// </summary>
        USER_RESERVED_7 = 17,

        /// <summary>
        /// 预留错误码8
        /// </summary>
        USER_RESERVED_8 = 18,

        /// <summary>
        /// 用户自定义错误码
        /// </summary>
        USER_CUSTOM = 99,
    }
}
