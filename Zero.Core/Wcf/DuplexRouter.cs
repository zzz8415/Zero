using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace Zero.Core.Wcf
{
    /// <summary>
    /// Wcf路由器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DuplexRouter<T> where T : class
    {
        private Random random = new Random();

        private HostSection section = null;

        private DuplexNodeClient<T> CreateServer<TCallback>(RouteHost host, TCallback callback)
        {
            if (section.Binding != null)
            {
                return new DuplexNodeClient<T>(new InstanceContext(callback), section.Binding, new EndpointAddress(host.Address));
            }
            return new DuplexNodeClient<T>(new InstanceContext(callback), serviceName, new EndpointAddress(host.Address));
        }

        private string serviceName = string.Empty;

        /// <summary>
        /// Wcf路由器
        /// </summary>
        /// <param name="serviceName"></param>
        private DuplexRouter(string serviceName)
        {
            this.serviceName = serviceName;
            this.section = ConfigurationManager.GetSection(serviceName) as HostSection;
        }

        private static object _locker = new object();

        private static DuplexRouter<T> instance = null;

        /// <summary>
        /// 路由实例
        /// </summary>
        public static DuplexRouter<T> Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (_locker)
                    {
                        if (instance == null)
                        {
                            instance = new DuplexRouter<T>(typeof(T).Name.Substring(1));
                        }
                    }
                }
                return instance;
            }
        }

        /// <summary>
        /// 创建渠道
        /// </summary>
        /// <returns></returns>
        public T CreateChannel<TCallback>(TCallback callback)
        {
            var host = section.Hosts[random.Next(section.Hosts.Count)];
            return CreateServer(host, callback).ChannelFactory.CreateChannel();
        }

        /// <summary>
        /// 根据传入的host创建渠道
        /// </summary>
        /// <returns></returns>
        public T CreateChannel<TCallback>(TCallback callback, string host)
        {
            foreach (RouteHost routeHost in section.Hosts)
            {
                if (routeHost.Address.IndexOf(host, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return CreateServer(routeHost, callback).ChannelFactory.CreateChannel();
                }
            }
            return null;      
        }
    }
}
