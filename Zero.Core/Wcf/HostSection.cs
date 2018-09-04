using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.Text;

namespace Zero.Core.Wcf
{
    /// <summary>
    /// Host节点
    /// </summary>
    public class HostSection : ConfigurationSection
    {
        /// <summary>
        /// Host集合
        /// </summary>
        [ConfigurationProperty("hosts")]
        public HostCollection Hosts
        {
            get { return (HostCollection)this["hosts"]; }
        }

        /// <summary>
        /// 绑定配置名
        /// </summary>
        [ConfigurationProperty("bindingConfiguration")]
        private string bindingConfiguration
        {
            get { return this["bindingConfiguration"].ToString(); }
        }


        [ConfigurationProperty("binding")]
        private string binding
        {
            get { return this["binding"].ToString(); }
        }

        /// <summary>
        /// 绑定类型
        /// </summary>
        public Binding Binding
        {
            get
            {
                Binding bind = null;
                var section = (BindingsSection)ConfigurationManager.GetSection("system.serviceModel/bindings");
                if (section == null)
                {
                    return bind;
                }
                var bindings = section.BindingCollections.FirstOrDefault(x => x.BindingName.Equals(binding, StringComparison.Ordinal));
                if (bindings == null)
                {
                    return bind;
                }
                var bindingConfig = bindings.ConfiguredBindings.First(x => x.Name.Equals(bindingConfiguration));
                if (bindingConfig == null)
                {
                    return bind;
                }
                switch (binding)
                {
                    case "basicHttpBinding":
                        bind = new BasicHttpBinding();
                        break;
                    case "netTcpBinding":
                        bind = new NetTcpBinding();
                        break;
                    case "wsHttpBinding":
                        bind = new WSHttpBinding();
                        break;
                    case "wsDualHttpBinding":
                        bind = new WSDualHttpBinding();
                        break;
                    case "webHttpBinding":
                        bind = new WebHttpBinding();
                        break;
                }
                if (bind != null) bindingConfig.ApplyConfiguration(bind);
                return bind;
            }
        }
    }
}
