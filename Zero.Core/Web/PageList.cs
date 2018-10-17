using System;
using System.Collections.Generic;

namespace Zero.Core.Web
{
    /// <summary>
    /// 分页列表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class PageList<T>
    {
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        public PageList(int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="list"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="recordCount"></param>
        public PageList(List<T> list, int pageIndex, int pageSize, long recordCount)
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
            RecordCount = recordCount;
            List = list;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="list"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="recordCount"></param>
        /// <param name="extend"></param>
        public PageList(List<T> list, int pageIndex, int pageSize, long recordCount, object extend)
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
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
        public bool IsLastPage => List.Count < PageSize || PageCount <= PageIndex;

        /// <summary>
        /// 扩展对象
        /// </summary>
        public object Extend { get; set; }
    }
}
