using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Zero.Core.Wcf
{
    /// <summary>
    /// 表示包含一个子元素集合的配置元素。
    /// </summary>
    [ConfigurationCollection(typeof(RouteHost), CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public class HostCollection : ConfigurationElementCollection
    {
        /// <summary>
        /// 当前索引器
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public RouteHost this[int index]
        {
            get { return (RouteHost)base.BaseGet(index); }
            set
            {
                if (base.BaseGet(index) != null)
                {
                    base.BaseRemoveAt(index);
                }
                base.BaseAdd(index, value);
            }
        }

        /// <summary>
        /// 当前索引器
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public new RouteHost this[string name]
        {
            get { return (RouteHost)base.BaseGet(name); }
        }

        /// <summary>
        /// 创建新节点
        /// </summary>
        /// <returns></returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new RouteHost();
        }

        /// <summary>
        /// 获取节点键值
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as RouteHost).Address;
        }

    }
}
