using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;
using System.Text;

namespace Zero.NETCore.Web
{
    /// <summary>
    /// Web客户端信息
    /// </summary>
    public class WebClient
    {
        /// <summary>
        /// http请求信息
        /// </summary>
        public HttpRequest Request { get; set; }

        /// <summary>
        /// 缓存
        /// </summary>
        public IMemoryCache MemoryCache { get; set; }

        /// <summary>
        /// 配置文件
        /// </summary>
        public IConfiguration Configuration { get; set; }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="request"></param>
        public WebClient(HttpRequest request)
        {
            Request = request;
        }

        #region 请求相关
        /// <summary>
        /// 获取上传的字符串值
        /// </summary>
        /// <param name="paramName"></param>
        /// <returns></returns>
        public string GetParam(string paramName)
        {
            if (Request.Query.TryGetValue(paramName, out StringValues value) || 
                (Request.HasFormContentType && Request.Form.TryGetValue(paramName, out value)))
            {
                return value;
            }
            return null;
        }

        private string _postData = null;

        /// <summary>
        /// Post数据流
        /// </summary>
        public string PostData
        {
            get
            {
                if (_postData == null)
                {
                    byte[] bytes = new byte[Request.Body.Length];
                    Request.Body.Read(bytes, 0, bytes.Length);
                    _postData = Encoding.UTF8.GetString(bytes);
                }
                return _postData;
            }
        }
        #endregion

        #region 缓存相关
        /// <summary>
        /// 生成Key
        /// </summary>
        /// <param name="prefixKey"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public string RenderCacheKey(string prefixKey, params string[] args)
        {
            return $"{prefixKey}.{string.Join(".", args)}";
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T GetCache<T>(string key)
        {
            return MemoryCache.Get<T>(key);
        }

        /// <summary>
        /// 设置缓存,默认15分钟
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="cache"></param>
        public void SetCache<T>(string key, T cache, int minutes = 15)
        {
            MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions()
            // Keep in cache for this time, reset time if accessed.
            .SetSlidingExpiration(TimeSpan.FromMinutes(minutes));

            MemoryCache.Set(key, cache, cacheEntryOptions);
        }

        /// <summary>
        /// 获取缓存,如果不存在则创建新缓存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="func"></param>
        /// <param name="minutes"></param>
        /// <returns></returns>
        public T GetCache<T>(string key, Func<T> func, int minutes = 15)
        {
            return MemoryCache.GetOrCreate<T>(key, entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(minutes);
                return func();
            });
        }
        #endregion

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
