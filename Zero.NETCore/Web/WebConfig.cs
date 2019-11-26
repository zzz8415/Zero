using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zero.NETCore.Attribute;

namespace Zero.NETCore.Web
{
    /// <summary>
    /// 配置帮助类
    /// </summary>
    [Inject(OptionsLifetime = ServiceLifetime.Singleton)]
    public class WebConfig
    {
        /// <summary>
        /// 配置文件
        /// </summary>
        public IConfiguration Configuration { get; set; }

        /// <summary>
        /// 配置初始化
        /// </summary>
        /// <param name="configuration"></param>
        public WebConfig(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        #region 配置相关
        /// <summary>
        /// 获取单个配置
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public T GetValue<T>(string key, T defaultValue = default)
        {
            return Configuration.GetValue(key, defaultValue);
        }

        /// <summary>
        /// 获取配置对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public T Get<T>(string key) where T : new()
        {
            return Configuration.GetSection(key).Get<T>();
        }

        /// <summary>
        /// 设置配置
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Set<T>(string key, T value)
        {
            Configuration.Bind(key, value);
        }
        #endregion
    }
}
