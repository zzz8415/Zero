using System;
using System.Collections.Generic;
using Zero.Core.Web.Request;

namespace Zero.Core.Web
{
    /// <summary>
    /// 分页列表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class PageList<T>
    {
        public PageList() { }
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="list"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="recordCount"></param>
        /// <param name="extend"></param>
        public PageList(List<T> list, int pageIndex, int pageSize, long recordCount, object extend = null)
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
            RecordCount = recordCount;
            List = list;
            Extend = extend;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="list"></param>
        /// <param name="pageRequest"></param>
        /// <param name="recordCount"></param>
        /// <param name="extend"></param>
        public PageList(List<T> list, PageRequest pageRequest, long recordCount, object extend = null)
        {
            PageIndex = pageRequest.PageIndex;
            PageSize = pageRequest.PageSize;
            RecordCount = recordCount;
            List = list;
            Extend = extend;
        }

        /// <summary>
        /// 全部数据总数
        /// </summary>
        public long RecordCount { get; set; }

        /// <summary>
        /// 页面总数
        /// </summary>
        public int PageCount => PageSize > 0 ? (int)Math.Ceiling((double)RecordCount / PageSize) : 0;

        /// <summary>
        /// 当前页面索引
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// 页面数据数量
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 数据列表
        /// </summary>
        public List<T> List { get; set; }

        /// <summary>
        /// 是否最后一页
        /// </summary>
        public bool IsLastPage => PageCount <= PageIndex;

        /// <summary>
        /// 扩展对象
        /// </summary>
        public object Extend { get; set; }
    }
}
