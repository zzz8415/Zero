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
    public class Router<T> where T : class
    {
        private Random random = new Random();

        private Dictionary<int, NodeClient<T>> servers = null;

        private void SetServers(HostSection section)
        {
            servers = new Dictionary<int, NodeClient<T>>();
            for (var i = 0; i < section.Hosts.Count; i++)
            {
                if (section.Binding != null)
                {
                    servers[i] = new NodeClient<T>(section.Binding, new EndpointAddress(section.Hosts[i].Address));
                }
                else
                {
                    servers[i] = new NodeClient<T>(serviceName, new EndpointAddress(section.Hosts[i].Address));
                }
            }
        }

        private string serviceName = string.Empty;

        /// <summary>
        /// Wcf路由器
        /// </summary>
        /// <param name="serviceName"></param>
        private Router(string serviceName)
        {
            this.serviceName = serviceName;
            var section = ConfigurationManager.GetSection(serviceName) as HostSection;
            SetServers(section);
        }

        private static object _locker = new object();

        private static Router<T> instance = null;

        /// <summary>
        /// 路由实例
        /// </summary>
        public static Router<T> Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (_locker)
                    {
                        if (instance == null)
                        {
                            instance = new Router<T>(typeof(T).Name.TrimStart('I'));
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
        public T CreateChannel()
        {
            var server = servers[random.Next(servers.Count)];
            return server.ChannelFactory.CreateChannel();
        }

        /// <summary>
        /// 根据传入的host创建渠道
        /// </summary>
        /// <returns></returns>
        public T CreateChannel(string host)
        {
            var server = servers.Values.FirstOrDefault(x => x.Endpoint.Address.Uri.Host.Equals(host, StringComparison.Ordinal));
            return server.ChannelFactory.CreateChannel();
        }
    }
}
