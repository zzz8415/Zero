using Microsoft.Extensions.Caching.Memory;
using System;

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
        public string RenderKey(string prefixKey, params string[] args)
        {
            return $"{prefixKey}.{string.Join(".", args)}";
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T Get<T>(string key)
        {
            return MemoryCache.Get<T>(key);
        }

        /// <summary>
        /// 设置缓存,默认15分钟
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="cache"></param>
        public void Set<T>(string key, T value, int minutes = 15, bool isPenetrate = true)
        {
            MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions()
            // Keep in cache for this time, reset time if accessed.
            .SetSlidingExpiration(TimeSpan.FromMinutes(minutes));

            if (!isPenetrate || !IsDefaultValue(value))
            {
                MemoryCache.Set(key, minutes, cacheEntryOptions);
            }
        }

        /// <summary>
        /// 获取缓存,如果不存在则创建新缓存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="func"></param>
        /// <param name="minutes"></param>
        /// <returns></returns>
        public T Get<T>(string key, Func<T> func, int minutes = 15, bool isPenetrate = true)
        {
            T value = MemoryCache.Get<T>(key);
            if (isPenetrate && IsDefaultValue(value))
            {
                return value;
            }

            value = func();
            if (!isPenetrate || !IsDefaultValue(value))
            {
                MemoryCache.Set(key, value);
            }
            
            return value;
        }

        /// <summary>
        /// 是否默认值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        private bool IsDefaultValue<T>(T value)
        {
            return value == null || value.Equals(default(T));
        }

        /// <summary>
        /// 移除缓存
        /// </summary>
        /// <param name="key"></param>
        public void Remove(string key)
        {
            MemoryCache.Remove(key);
        }
        #endregion
    }
}
