using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using Zero.Core.Attribute;
using Zero.Core.Inject;

namespace Zero.Core.Extensions
{
    /// <summary>
    /// 服务提供扩展方法
    /// </summary>
    public static class ServiceExtensions
    {
        /// <summary>
        /// 注入程序集
        /// </summary>
        /// <param name="service"></param>
        /// <param name="assemblyName">要注入程序集的名称</param>
        public static void AddAssembly(this IServiceCollection service, string assemblyName)
        {
            string binPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            Assembly assembly = Assembly.LoadFrom(Path.Combine(binPath, assemblyName + ".dll"));

            List<TypeInfo> list = assembly.DefinedTypes.ToList();

            foreach (TypeInfo info in list)
            {
                if (info.IsAbstract)
                {
                    continue;
                }

                InjectAttribute inject = info.GetCustomAttribute<InjectAttribute>();

                if (inject == null)
                {
                    continue;
                }

                List<Type> interfaces = info.ImplementedInterfaces.ToList();

                foreach (var inter in interfaces)
                {
                    service.Add(new ServiceDescriptor(inter, info, inject.OptionsLifetime));
                }

                service.Add(new ServiceDescriptor(info, info, inject.OptionsLifetime));
            }

        }

        /// <summary>
        /// 注入Zero.Core程序集
        /// </summary>
        /// <param name="service"></param>
        public static void AddZeroNetCoreAssembly(this IServiceCollection service)
        {
            service.AddAssembly("Zero.Core");
        }

        ///// <summary>
        ///// 注入Zero.Core.Redis程序集,注入之前请确认是否nuget上安装了Zero.Core.Redis包
        ///// </summary>
        ///// <param name="service"></param>
        //public static void AddZeroNETCoreRedisAssembly(this IServiceCollection service)
        //{
        //    service.AddAssembly("Zero.Core.Redis");
        //}
    }
}
