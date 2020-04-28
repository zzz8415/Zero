using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zero.NETCore.Web.Request;

namespace Zero.NETCore.Web
{
    /// <summary>
    /// 移动端分页列表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class AppPageList<T>
    {
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="list"></param>
        /// <param name="pageSize"></param>
        public AppPageList(List<T> list, int pageSize)
        {
            this.List = list;

            this.IsLastPage = this.List == null || this.List.Count <= pageSize;

            if (!this.IsLastPage)
            {
                this.List = this.List.Take(pageSize).ToList();
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="list"></param>
        /// <param name="isLastPage"></param>
        public AppPageList(List<T> list, bool isLastPage)
        {
            this.List = list;

            this.IsLastPage = isLastPage;
        }


        /// <summary>
        /// 数据列表
        /// </summary>
        public List<T> List { get; set; }

        /// <summary>
        /// 是否最后一页
        /// </summary>
        public bool IsLastPage { get; set; }
    }
}
