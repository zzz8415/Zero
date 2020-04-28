using System;
using System.Collections.Generic;
using System.Text;

namespace Zero.NETCore.Web.Request
{
    public class PageRequest
    {
        /// <summary>
        /// 页码 1开始
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// 每页显示多少条
        /// </summary>
        public int PageSize { get; set; }

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
