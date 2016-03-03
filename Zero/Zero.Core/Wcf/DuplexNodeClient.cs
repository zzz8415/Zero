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
    public class DuplexNodeClient<T> : DuplexClientBase<T> where T : class
    {
        /// <summary>
        /// 节点客户信息
        /// </summary>
        /// <param name="callbackInstance"></param>
        /// <param name="serviceName"></param>
        public DuplexNodeClient(InstanceContext callbackInstance, string serviceName)
            : base(callbackInstance, serviceName)
        {
        }

        /// <summary>
        /// 节点客户信息
        /// </summary>
        /// <param name="callbackInstance"></param>
        /// <param name="serviceName"></param>
        /// <param name="address"></param>
        public DuplexNodeClient(InstanceContext callbackInstance, string serviceName, EndpointAddress address)
            : base(callbackInstance, serviceName, address)
        {
        }

        /// <summary>
        /// 节点客户信息
        /// </summary>
        /// <param name="callbackInstance"></param>
        /// <param name="binding"></param>
        /// <param name="address"></param>
        public DuplexNodeClient(InstanceContext callbackInstance, Binding binding, EndpointAddress address)
            : base(callbackInstance, binding, address)
        {
        }

    }
}
