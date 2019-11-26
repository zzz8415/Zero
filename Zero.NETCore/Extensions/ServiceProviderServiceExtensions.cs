using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Zero.NETCore.Attribute;

namespace Zero.NETCore.Extensions
{
    public static class ServiceProviderServiceExtensions
    {
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
    }
}
