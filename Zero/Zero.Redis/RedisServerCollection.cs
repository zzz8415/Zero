using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Zero.Redis
{
    /// <summary>
    /// Redis Host 配置
    /// </summary>
    [ConfigurationCollection(typeof(RedisServerSection), CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public class RedisHostCollection : ConfigurationElementCollection
    {
        /// <summary>
        /// 索引
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public RedisServerSection this[int index]
        {
            get { return (RedisServerSection)base.BaseGet(index); }
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
        /// 索引
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public new RedisServerSection this[string name]
        {
            get { return (RedisServerSection)base.BaseGet(name); }
        }

        /// <summary>
        /// 创建新节点
        /// </summary>
        /// <returns></returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new RedisServerSection();
        }

        /// <summary>
        /// 获取节点值
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as RedisServerSection).Host;
        }

    }
}
