using Microsoft.Extensions.Configuration;

namespace Zero.NETCore.Web
{
    /// <summary>
    /// 配置帮助类
    /// </summary>
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
        /// 获取配置
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public T GetConfig<T>(string key, T defaultValue = default(T))
        {
            return Configuration.GetValue<T>(key, defaultValue);
        }

        /// <summary>
        /// 设置配置
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetConfig<T>(string key, T value)
        {
            Configuration.Bind(key, value);
        }
        #endregion
    }
}
