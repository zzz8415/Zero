using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Zero.Core.Attribute;

namespace Zero.Core.Inject
{
    /// <summary>
    /// 缓存帮助类
    /// </summary>
    /// <remarks>
    /// 缓存初始化
    /// </remarks>
    /// <param name="memoryCache"></param>
    [Inject(OptionsLifetime = ServiceLifetime.Singleton)]
    public class WebCache(IMemoryCache memoryCache)
    {

        /// <summary>
        /// 缓存
        /// </summary>
        public IMemoryCache MemoryCache { get; set; } = memoryCache;

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
        /// <param name="value"></param>
        /// <param name="minutes"></param>
        /// <param name="isPenetrate"></param>
        /// <returns></returns>
        public T Set<T>(string key, T value, int minutes = 15, bool isPenetrate = true)
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions()
            // Keep in cache for this time, reset time if accessed.
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(minutes));

            // 如果非穿透 或者 值不为空 ,则保存到缓存
            if (!isPenetrate || !IsDefaultValue(value))
            {
                return MemoryCache.Set(key, value, cacheEntryOptions);
            }
            return value;
        }

        /// <summary>
        /// 获取缓存,如果不存在则创建新缓存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="func"></param>
        /// <param name="minutes"></param>
        /// <param name="isPenetrate"></param>
        /// <returns></returns>
        public T Get<T>(string key, Func<T> func, int minutes = 15, bool isPenetrate = true)
        {
            // 如果不穿透,使用默认方法
            if (!isPenetrate)
            {
                MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions()
               // Keep in cache for this time, reset time if accessed.
               .SetAbsoluteExpiration(TimeSpan.FromMinutes(minutes));
                return MemoryCache.GetOrCreate(key, x =>
                {
                    return func();
                });
            }

            // 如果要穿透,先获取值
            T value = MemoryCache.Get<T>(key);
            // 如果非空值,返回数据
            if (!IsDefaultValue(value))
            {
                return value;
            }

            // 调用回调方法
            value = func();

            return Set(key, value, minutes, isPenetrate);
        }

        /// <summary>
        /// 清除所有缓存
        /// </summary>
        public void Clear()
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            object entries = MemoryCache.GetType().GetField("_entries", flags).GetValue(MemoryCache);
            if (entries is not IDictionary cacheItems)
            {
                return;
            }
            foreach (DictionaryEntry cacheItem in cacheItems)
            {
                Remove(cacheItem.Key.ToString());
            }
        }

        /// <summary>
        /// 是否默认值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        private bool IsDefaultValue<T>(T value)
        {
            return value == null || value.Equals(default);
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
