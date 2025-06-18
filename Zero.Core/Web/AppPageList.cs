using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zero.Core.Web.Request;

namespace Zero.Core.Web
{
    /// <summary>
    /// 移动端分页列表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class AppPageList<T>
    {
        public AppPageList() { }

        /// <summary>
        ///  初始化,获取数量建议pageRequest.AppPageGetCount(pageSize+1),这样可以准确判断是否还有下一页
        /// </summary>
        /// <param name="list"></param>
        /// <param name="pageRequest"></param>
        /// <param name="extend"></param>
        public AppPageList(List<T> list, PageRequest pageRequest, object extend = null)
        {
            this.List = list;

            this.IsLastPage = this.List == null || this.List.Count <= pageRequest.PageSize;

            if (!this.IsLastPage)
            {
                this.List = this.List.Take(pageRequest.PageSize).ToList();
            }

            this.Extend = extend;
        }

        /// <summary>
        /// 初始化,获取数量建议pageSize+1,这样可以准确判断是否还有下一页
        /// </summary>
        /// <param name="list"></param>
        /// <param name="pageSize"></param>
        public AppPageList(List<T> list, int pageSize, object extend = null)
        {
            this.List = list;

            this.IsLastPage = this.List == null || this.List.Count <= pageSize;

            if (!this.IsLastPage)
            {
                this.List = this.List.Take(pageSize).ToList();
            }

            this.Extend = extend;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="list"></param>
        /// <param name="isLastPage"></param>
        public AppPageList(List<T> list, bool isLastPage, object extend = null)
        {
            this.List = list;

            this.IsLastPage = isLastPage;

            this.Extend = extend;
        }


        /// <summary>
        /// 数据列表
        /// </summary>
        public List<T> List { get; set; }

        /// <summary>
        /// 是否最后一页
        /// </summary>
        public bool IsLastPage { get; set; }

        /// <summary>
        /// 扩展对象
        /// </summary>
        public object Extend { get; set; }
    }
}
