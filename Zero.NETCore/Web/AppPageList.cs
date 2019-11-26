using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zero.NETCore.Web
{
    [Serializable]
    public class AppPageList<T>
    {
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="list"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="RecordCount"></param>
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
        /// 数据列表
        /// </summary>
        public List<T> List { get; set; }

        /// <summary>
        /// 是否最后一页
        /// </summary>
        public bool IsLastPage { get; set; }
    }
}
