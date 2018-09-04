using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceModel;

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
            foreach (RouteHost host in section.Hosts)
            {
                if (section.Binding != null)
                {
                    servers[host.Index] = new NodeClient<T>(section.Binding, new EndpointAddress(host.Address));
                }
                else
                {
                    servers[host.Index] = new NodeClient<T>(serviceName, new EndpointAddress(host.Address));
                }
            }
        }

        private readonly string serviceName = string.Empty;

        /// <summary>
        /// Wcf路由器
        /// </summary>
        /// <param name="serviceName"></param>
        private Router(string serviceName)
        {
            this.serviceName = serviceName;
            HostSection section = ConfigurationManager.GetSection(serviceName) as HostSection;
            SetServers(section);
        }

        private static readonly object _locker = new object();

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
                            instance = new Router<T>(typeof(T).Name.Substring(1));
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
            var key = servers.Keys.ToList()[random.Next(servers.Count)];
            NodeClient<T> server = servers[key];
            return server.ChannelFactory.CreateChannel();
        }

        /// <summary>
        /// 根据传入的host创建渠道
        /// </summary>
        /// <returns></returns>
        public T CreateChannel(string host)
        {
            NodeClient<T> server = servers.Values.FirstOrDefault(x => x.Endpoint.Address.Uri.Authority.IndexOf(host, StringComparison.Ordinal) >= 0);
            return server.ChannelFactory.CreateChannel();
        }

        /// <summary>
        /// 根据传入的索引创建渠道
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public T CreateChannel(int index)
        {
            NodeClient<T> server = servers[index];
            return server.ChannelFactory.CreateChannel();
        }
    }
}
