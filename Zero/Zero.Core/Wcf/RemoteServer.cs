using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using Zero.Core.Util;

namespace Zero.Core.Wcf
{
    /// <summary>
    /// 远程服务,需设置nameof(T)_Host域名和nameof(T)_Port端口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RemoteServer<T> : ServiceHost where T : class 
    {
        private RemoteServer(Type type)
            : base(type)
        {
        }

        private RemoteServer(Type type, params Uri[] baseAddresses)
            : base(type, baseAddresses)
        {
        }

        private static RemoteServer<T> instance = null;

        /// <summary>
        /// 单实例
        /// </summary>
        public static RemoteServer<T> Instance
        {
            get
            {
                if (instance == null)
                {
                    var type = typeof(T);
                    var address = CustomHelper.GetValue(type.Name + "_Address");
                    if (string.IsNullOrEmpty(address))
                    {
                        throw new ArgumentNullException("必须在Custom.config配置文件中指定服务路径[" + type.Name + "_Address]的值.");
                    }

                    instance = new RemoteServer<T>(type, new Uri(address));
                    instance.Opened += delegate
                    {
                        LogHelper.WriteInfo(string.Format("{0}远程服务器已启动.", type.Name));
                    };
                }
                return instance;
            }
        }
    }
}
