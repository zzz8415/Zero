using System;
using System.Collections.Generic;
using System.Text;

namespace Zero.Core.Web.Request
{
    /// <summary>
    /// 分页请求
    /// </summary>
    public class PageRequest
    {
        /// <summary>
        /// 页码 1开始
        /// </summary>
        public int PageIndex { get; set; } = 1;

        /// <summary>
        /// 每页显示多少条,默认24条
        /// </summary>
        public int PageSize { get; set; } = 24;
        
        /// <summary>
        /// app页面每次多获取一个用于判断是否还有下一页
        /// </summary>
        public int AppPageGetCount => PageSize + 1;

        /// <summary>
        /// 跳过多少页
        /// </summary>
        public int PageSkip
        {
            get
            {
                return (this.PageIndex - 1) * this.PageSize;
            }
        }
    }
}
