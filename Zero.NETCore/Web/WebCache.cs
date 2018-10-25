using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Text;

namespace Zero.NETCore.Web
{
    /// <summary>
    /// 缓存帮助类
    /// </summary>
    public class WebCache
    {

        /// <summary>
        /// 缓存
        /// </summary>
        public IMemoryCache MemoryCache { get; set; }

        /// <summary>
        /// 缓存初始化
        /// </summary>
        /// <param name="memoryCache"></param>
        public WebCache(IMemoryCache memoryCache)
        {
            MemoryCache = memoryCache;
        }

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
    }
}
