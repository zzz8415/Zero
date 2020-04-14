using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Zero.NETCore.Attribute
{
    /// <summary>
    /// 注入服务属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class InjectAttribute : System.Attribute
    {
        /// <summary>
        /// 什么周期类型,默认Scoped,线程内有效
        /// </summary>
        public ServiceLifetime OptionsLifetime { get; set; } = ServiceLifetime.Scoped;
    }
}
