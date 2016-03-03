using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

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
        [ConfigurationProperty("address")]
        public string Address
        {
            get { return this["address"].ToString(); }
            set { this["address"] = value; }
        }
    }
}
