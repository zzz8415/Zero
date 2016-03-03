using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Zero.Core.Web;
using Zero.Core.Util;
using Zero.Core.Extensions;

namespace Zero.DataAccess
{
    /// <summary>
    /// 数据库配置帮助类
    /// </summary>
    public class DBConfigHelper
    {
        private static string configPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"db.config"));

        private static DBConfigHelper instance = null;
        /// <summary>
        /// 获取数据库配置的单一实例,当文件修改时自动更新(文件缓存)
        /// 调用此类请捕获异常
        /// </summary>
        public static DBConfigHelper Instance
        {
            get
            {
                return CacheHelper.GetByFileDependency("DBConfigHelper", configPath, () =>
                    {
                        if (instance == null)
                        {
                            instance = new DBConfigHelper();
                            instance.Providers = new Dictionary<string, ConnectionProvider>();
                        }
                        instance.Init();
                        return instance;
                    });
            }
        }

        private Dictionary<string, ConnectionProvider> Providers { get; set; }

        private void Init()
        {
            if ((configPath == null) || !File.Exists(configPath))
            {
                throw new FileNotFoundException("指定的配置文件不存在", configPath);
            }

            var doc = XDocument.Load(configPath);

            Providers = doc.Root.Elements().ToDictionary(
                    x => x.Name.ToString(),
                    x => {
                        var provider = new ConnectionProvider{
                            ConnectInfos = new Dictionary<int,ConnectInfo>()
                        };
                        foreach(var connection in x.Elements("Connection"))
                        {
                            var connectInfo =  new ConnectInfo
                                {
                                    ReadConnectionString = CryptoHelper.DES_Decrypt(connection.Element("ReadString").Value),
                                    WriteConnectionString = CryptoHelper.DES_Decrypt(connection.Element("WriteString").Value),
                                    BackupReadConnectionString = connection.Element("BackupReadString") == null ? string.Empty : CryptoHelper.DES_Decrypt(connection.Element("BackupReadString").Value)
                                };

                            if(connection.Attribute("LogicRegion") == null)
                            {
                                provider.ConnectInfos[0] = connectInfo;
                            }
                            else{
                                var logicRegions = connection.Attribute("LogicRegion").Value.Split('|');
                                foreach(var region in logicRegions)
                                {
                                    provider.ConnectInfos[region.ToInt32(0)] = connectInfo; 
                                }
                            }
                        }
                        return provider;
                    }
                );
        }

        /// <summary>
        /// 根据传入的名称获取连接授权类,如果不存在返回null类型
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ConnectionProvider GetProvider(string name)
        {
            ConnectionProvider provider = null;
            if (Providers != null && Providers.TryGetValue(name, out provider))
            {
                return provider;
            }
            return provider;
        }

        /// <summary>
        /// 根据传入的名称获取读数据库连接串
        /// </summary>
        /// <param name="name"></param>
        /// <param name="region"></param>
        /// <returns></returns>
        public string GetReadString(string name, int region = 0)
        {
            return GetProvider(name).ConnectInfos[region].ReadConnectionString;
        }

        /// <summary>
        /// 根据传入的名称获取写数据库连接串
        /// </summary>
        /// <param name="name"></param>
        /// <param name="region"></param>
        /// <returns></returns>
        public string GetWriteString(string name, int region = 0)
        {
            return GetProvider(name).ConnectInfos[region].WriteConnectionString;
        }
    }
}
