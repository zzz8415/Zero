using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;

namespace Zero.Core.Wcf
{
    /// <summary>
    /// 节点客户信息
    /// </summary>
    public class NodeClient<T> : ClientBase<T> where T : class
    {
        /// <summary>
        /// 节点客户信息
        /// </summary>
        /// <param name="serviceName"></param>
        public NodeClient(string serviceName)
            : base(serviceName)
        {
        }

        /// <summary>
        /// 节点客户信息
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="address"></param>
        public NodeClient(string serviceName, EndpointAddress address)
            : base(serviceName, address)
        {
        }

        /// <summary>
        /// 节点客户信息
        /// </summary>
        /// <param name="binding"></param>
        /// <param name="address"></param>
        public NodeClient(Binding binding, EndpointAddress address)
            : base(binding, address)
        {
        }

    }
}
