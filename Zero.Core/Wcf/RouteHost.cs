using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Zero.Core.Extensions;

namespace Zero.Core.Wcf
{
    /// <summary>
    /// 路由Host
    /// </summary>
    public class RouteHost : ConfigurationSection
    {
        /// <summary>
        /// 用户账号域地址
        /// </summary>
        [ConfigurationProperty("index")]
        public int Index
        {
            get { return this["index"].ToInt32(0); }
            set { this["index"] = value; }
        }

        /// <summary>
        /// 用户账号域地址
        /// </summary>
        [ConfigurationProperty("address")]
        public string Address
        {
            get { return this["address"].ToString(); }
            set { this["address"] = value; }
        }
    }
}
