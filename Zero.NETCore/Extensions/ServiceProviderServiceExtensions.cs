using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Zero.NETCore.Attribute;
using Zero.NETCore.Inject;

namespace Zero.NETCore.Extensions
{
    /// <summary>
    /// 服务提供扩展方法
    /// </summary>
    public static class ServiceProviderServiceExtensions
    {
        /// <summary>
        /// 注入程序集
        /// </summary>
        /// <param name="service"></param>
        /// <param name="assemblyName">要注入程序集的名称</param>
        public static void AddAssembly(this IServiceCollection service, string assemblyName)
        {
            try
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
            catch (Exception ex)
            {
                new LogClient().WriteException(ex);
            }
        }

        /// <summary>
        /// 注入Zero.NETCore程序集
        /// </summary>
        /// <param name="service"></param>
        public static void AddZeroNetCoreAssembly(this IServiceCollection service)
        {
            service.AddAssembly("Zero.NETCore");
        }

        ///// <summary>
        ///// 注入Zero.NETCore.Redis程序集,注入之前请确认是否nuget上安装了Zero.NETCore.Redis包
        ///// </summary>
        ///// <param name="service"></param>
        //public static void AddZeroNETCoreRedisAssembly(this IServiceCollection service)
        //{
        //    service.AddAssembly("Zero.NETCore.Redis");
        //}
    }
}
