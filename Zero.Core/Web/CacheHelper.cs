/*
 * 修改日期：2009-02-25
 * 修 改 人：朱自立
 * 修改内容：重写CacheHelper类
 * 修改原因：原类缺少弹性过期的缓存设置
 * 
 */
using System;
using System.Web;
using System.Web.Caching;
using System.Collections;
using System.Linq.Expressions;
using System.Text;
using System.IO;
using Zero.Core.Extensions;

namespace Zero.Core.Web
{
	/// <summary>
	/// CacheHelper Web缓存帮助类 添加,移除,读取缓存
	/// </summary>
    public partial class CacheHelper
    {
        /// <summary>
        /// 通过缓存键获取相应的内容
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="key">缓存键</param>
        /// <returns></returns>
        public static T Get<T>(string key)
        {
            return (T)HttpRuntime.Cache[key];
        }

        /// <summary>
        /// 试着取得缓存内容，如果有返回真，并返回取得后的值。没有则返回默认值。
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="key">缓存键</param>
        /// <param name="outValue">返回相应类型的值</param>
        /// <returns></returns>
        public static bool TryGetValue<T>(string key, out T outValue)
        {
            object obj = HttpRuntime.Cache[key];
            if (obj != null)
            {
                try
                {
                    outValue = (T)obj;
                    return true;
                }
                catch
                {
                    outValue = default(T);
                    return false;
                }
            }
            else
            {
                outValue = default(T);
                return false;
            }
        }

        /// <summary>
        /// 判断是否有此key的值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool Contains(string key)
        {
            if (HttpRuntime.Cache[key] != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }



        /// <summary>
        /// 新增，添加缓存(如果缓存键已经存在，则此方法调用无效)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="lNumofMilliSeconds"></param>
        public static void Set<T>(string key, T value, long lNumofMilliSeconds)
        {
            HttpRuntime.Cache.Add(key, value, null, DateTime.Now.AddMilliseconds(lNumofMilliSeconds), TimeSpan.Zero, CacheItemPriority.NotRemovable, null);
        }

        /// <summary>
        /// 新增，添加缓存(如果缓存键已经存在，则此方法调用无效)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="tspan"></param>
        public static void Set<T>(string key, T value, TimeSpan tspan)
        {
            HttpRuntime.Cache.Add(key, value, null, DateTime.Now.Add(tspan), TimeSpan.Zero, CacheItemPriority.NotRemovable, null);
        }


        /// <summary>
        /// 添加缓存，缓存存在时，覆盖
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="lNumofMilliSeconds"></param>
        public static void Insert<T>(string key, T value, long lNumofMilliSeconds)
        {
            HttpRuntime.Cache.Insert(key, value, null, DateTime.Now.AddMilliseconds(lNumofMilliSeconds), TimeSpan.Zero, CacheItemPriority.NotRemovable, null);
        }

        /// <summary>
        /// 添加缓存，缓存存在时，覆盖
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="tspan"></param>
        public static void Insert<T>(string key, T value, TimeSpan tspan)
        {
            HttpRuntime.Cache.Insert(key, value, null, DateTime.Now.Add(tspan), TimeSpan.Zero, CacheItemPriority.NotRemovable, null);
        }

        /// <summary>
        /// 清除所有缓存
        /// </summary>
        public static void Clear()
        {
            foreach (DictionaryEntry elem in HttpRuntime.Cache)
            {
                HttpRuntime.Cache.Remove(elem.Key.ToString());
            }
        }

        /// <summary>
        /// 移除相关关键字的缓存
        /// </summary>
        /// <param name="keyword">缓存关键字</param>
        public static void Remove(string keyword)
        {
            foreach (DictionaryEntry elem in HttpRuntime.Cache)
            {
                if (elem.Key.ToString().Contains(keyword))
                {
                    HttpRuntime.Cache.Remove(elem.Key.ToString());
                }
            }
        }

        /// <summary>
        /// 清除某项缓存
        /// </summary>
        /// <param name="key">缓存键</param>
        public static void RemoveAt(string key)
        {
            if (HttpRuntime.Cache[key] != null)
            {
                HttpRuntime.Cache.Remove(key);
            }
        }

        /// <summary>
        /// 获取绝对日期时间
        /// </summary>
        /// <param name="CacheTimeOption">缓存的时间长短</param>
        /// <param name="CacheExpirationOption"></param>
        /// <returns></returns>
        public static DateTime GetAbsoluteExpirationTime(CacheTimeOption CacheTimeOption, CacheExpirationOption CacheExpirationOption)
        {
            if (CacheExpirationOption == CacheExpirationOption.SlidingExpiration
                || CacheTimeOption == CacheTimeOption.NotRemovable)
            {
                return Cache.NoAbsoluteExpiration;
            }

            return DateTime.Now.AddMinutes((int)CacheTimeOption);
        }

        /// <summary>
        /// 获取弹性时间过期时间
        /// </summary>
        /// <param name="CacheTimeOption">缓存的时间长短</param>
        /// <param name="CacheExpirationOption"></param>
        /// <returns></returns>
        private static TimeSpan GetSlidingExpirationTime(CacheTimeOption CacheTimeOption, CacheExpirationOption CacheExpirationOption)
        {
            if (CacheExpirationOption == CacheExpirationOption.AbsoluteExpiration
                || CacheTimeOption == CacheTimeOption.NotRemovable)
            {
                return Cache.NoSlidingExpiration;
            }

            return TimeSpan.FromMinutes((int)CacheTimeOption);
        }


        #region 用CacheTimeOption的Set方法
        // 主调方法
        /// <summary>
        /// 添加缓存(如果缓存键已经存在，则此方法调用无效)
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="key">缓存键</param>
        /// <param name="value">值</param>
        /// <param name="CacheTimeOption">缓存的时间长短</param>
        /// <param name="CacheExpirationOption"></param>
        /// <param name="dependencies">缓存依赖项</param>
        /// <param name="cacheItemPriority">优先级</param>
        /// <param name="callback">回调函数</param>
        public static void Set<T>(string key, T value, CacheTimeOption CacheTimeOption, CacheExpirationOption CacheExpirationOption,
            CacheDependency dependencies, CacheItemPriority cacheItemPriority, CacheItemRemovedCallback callback)
        {
            if (!value.Equals(default(T)) && CacheTimeOption != CacheTimeOption.None)
            {
                DateTime absoluteExpiration = GetAbsoluteExpirationTime(CacheTimeOption, CacheExpirationOption);
                TimeSpan slidingExpiration = GetSlidingExpirationTime(CacheTimeOption, CacheExpirationOption);
                HttpRuntime.Cache.Add(key, value, dependencies, absoluteExpiration, slidingExpiration, cacheItemPriority, callback);
            }
        }

        //以下Set<T>为重载方法
        /// <summary>
        /// 添加缓存(如果缓存键已经存在，则此方法调用无效)
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="key">缓存键</param>
        /// <param name="value">值</param>
        /// <param name="CacheTimeOption">缓存的时间长短</param>
        /// <param name="CacheExpirationOption"></param>
        /// <param name="dependencies">缓存依赖项</param>
        /// <param name="cacheItemPriority">优先级</param>
        public static void Set<T>(string key, T value, CacheTimeOption CacheTimeOption, CacheExpirationOption CacheExpirationOption, 
            CacheDependency dependencies, CacheItemPriority cacheItemPriority)
        {
            Set(key, value, CacheTimeOption, CacheExpirationOption, dependencies, cacheItemPriority, null);
        }

        /// <summary>
        /// 添加缓存(如果缓存键已经存在，则此方法调用无效)
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="key">缓存键</param>
        /// <param name="value">值</param>
        /// <param name="CacheTimeOption">缓存时间</param>
        /// <param name="CacheExpirationOption">缓存过期时间类别（绝对/弹性）</param>
        /// <param name="dependencies">缓存依赖项</param>
        public static void Set<T>(string key, T value, CacheTimeOption CacheTimeOption, CacheExpirationOption CacheExpirationOption, 
            CacheDependency dependencies)
        {
            Set(key, value, CacheTimeOption, CacheExpirationOption, dependencies, CacheItemPriority.NotRemovable, null);
        }

        /// <summary>
        /// 添加缓存(如果缓存键已经存在，则此方法调用无效)
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="key">缓存键</param>
        /// <param name="value">值</param>
        /// <param name="CacheTimeOption">缓存时间</param>
        /// <param name="CacheExpirationOption">缓存过期时间类别（绝对/弹性）</param>
        public static void Set<T>(string key, T value, CacheTimeOption CacheTimeOption, CacheExpirationOption CacheExpirationOption)
        {
            Set(key, value, CacheTimeOption, CacheExpirationOption, null, CacheItemPriority.NotRemovable, null);
        }

        /// <summary>
        /// 添加缓存(如果缓存键已经存在，则此方法调用无效)
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="key">缓存键</param>
        /// <param name="value">值</param>
        /// <param name="callback">回调函数</param>
        /// <param name="CacheTimeOption">缓存时间</param>
        public static void Set<T>(string key, T value, CacheTimeOption CacheTimeOption, CacheItemRemovedCallback callback)
        {
            Set(key, value, CacheTimeOption, CacheExpirationOption.AbsoluteExpiration, null, CacheItemPriority.NotRemovable, callback);
        }

        /// <summary>
        /// 添加缓存(如果缓存键已经存在，则此方法调用无效)
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="key">缓存键</param>
        /// <param name="value">值</param>
        /// <param name="CacheTimeOption">缓存时间</param>
        public static void Set<T>(string key, T value, CacheTimeOption CacheTimeOption)
        {
            Set(key, value, CacheTimeOption, CacheExpirationOption.AbsoluteExpiration, null, CacheItemPriority.NotRemovable, null);
        }

        /// <summary>
        /// 添加缓存(如果缓存键已经存在，则此方法调用无效)
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="key">缓存键</param>
        /// <param name="value">值</param>
        public static void Set<T>(string key, T value)
        {
            Set(key, value, CacheTimeOption.Normal, CacheExpirationOption.AbsoluteExpiration, null, CacheItemPriority.NotRemovable, null);
        }
        #endregion

        #region 2010-04-26带更新事件缓存方法
        /// <summary>
        /// 缓存数据源更新委托
        /// </summary>
        /// <returns></returns>
        public delegate T RefreshCacheDataHandler<T>();
        /// <summary>
        /// 带ref int的缓存数据源更新委托，一般用于分页方法
        /// </summary>
        /// <param name="recordCount"></param>
        /// <returns></returns>
        public delegate T RefreshCacheDataWithRefParamHandler<T>(ref int recordCount);
        /// <summary>
        /// 带ref int的缓存数据源更新委托，一般用于分页方法
        /// </summary>
        /// <param name="recordCount"></param>
        /// <param name="recordCount2"></param>
        /// <returns></returns>
        public delegate T RefreshCacheDataWithRefParamHandler2<T>(ref int recordCount, ref int recordCount2);
        /// <summary>
        /// 带out int的缓存数据源更新委托，一般用于分页方法
        /// </summary>
        /// <param name="recordCount"></param>
        /// <returns></returns>
        public delegate T RefreshCacheDataWithOutParamHandler<T>(out int recordCount);
        ///// <summary>
        ///// 获取指定委托的缓存数据，如果该缓存不存在，则自动调用数据源委托生成缓存
        ///// </summary>
        ///// <typeparam name="T">缓存数据的类型</typeparam>
        ///// <param name="cacheTime">缓存时间</param>
        ///// <param name="callback">数据源委托</param>
        ///// <returns></returns>
        //public static T Get<T>(CacheTimeOption cacheTime, Expression<RefreshCacheDataHandler> callback) where T : class
        //{
        //    MethodCallExpression exp = callback.Body as MethodCallExpression;
        //    StringBuilder sbKey = new StringBuilder();
        //    sbKey.Append("AUTO_CACHE_KEY___");
        //    sbKey.Append(exp.Method.DeclaringType.FullName);
        //    sbKey.Append("_");
        //    sbKey.Append(exp.Method.Name);
        //    for (int i = 0; i < exp.Arguments.Count; i++)
        //    {
        //        sbKey.Append("_");
        //        if (exp.Arguments[i] is ConstantExpression)
        //        {
        //            sbKey.Append((exp.Arguments[i] as ConstantExpression).Value);
        //        }
        //        if (exp.Arguments[i] is MemberExpression)
        //        {
        //            sbKey.Append((exp.Arguments[i] as MemberExpression).Member);      //方法变量参数的值取法有问题，需要复杂运算，暂时搁浅。。
        //        }
        //    }
        //    string key = "";
        //    if (Contains(key))
        //    {
        //        return Get<T>(key);
        //    }
        //    T content = callback.Compile()() as T;
        //    Set<T>(key, content, cacheTime);
        //    return content;
        //}


        /// <summary>
        /// 获取指定key缓存数据，如果该缓存不存在，则自动调用数据源委托生成缓存
        /// </summary>
        /// <typeparam name="T">缓存数据的类型</typeparam>
        /// <param name="key">缓存key</param>
        /// <param name="cacheTime">缓存时间</param>
        /// <param name="callback">数据源委托</param>
        /// <param name="removedCallback">缓存失效时的回调函数</param>
        /// <returns></returns>
        public static T Get<T>(string key, CacheTimeOption cacheTime, RefreshCacheDataHandler<T> callback, 
            CacheItemRemovedCallback removedCallback) where T : class
        {
            // 不缓存时，直接返回
            if (cacheTime == CacheTimeOption.None)
                return callback();

            if (Contains(key))
            {
                return Get<T>(key);
            }
            T content = callback();
            Set(key, content, cacheTime, removedCallback);
            return content;
        }

        /// <summary>
        /// 获取指定key缓存数据，如果该缓存不存在，则自动调用数据源委托生成缓存
        /// </summary>
        /// <typeparam name="T">缓存数据的类型</typeparam>
        /// <param name="key">缓存key</param>
        /// <param name="cacheTime">缓存时间</param>
        /// <param name="callback">数据源委托</param>
        /// <returns></returns>
        public static T Get<T>(string key, CacheTimeOption cacheTime, RefreshCacheDataHandler<T> callback)
        {
            // 不缓存时，直接返回
            if (cacheTime == CacheTimeOption.None)
                return callback();

            if (Contains(key))
            {
                return Get<T>(key);
            }
            T content = callback();
            Set(key, content, cacheTime);
            return content;
        }
        /// <summary>
        /// 获取指定key缓存数据，如果该缓存不存在，则自动调用数据源委托生成缓存
        /// </summary>
        /// <typeparam name="T">缓存数据的类型</typeparam>
        /// <param name="key">缓存key</param>
        /// <param name="recordCountKey">记录总数的缓存key</param>
        /// <param name="recordCount">记录总数(ref)</param>
        /// <param name="cacheTime">缓存时间</param>
        /// <param name="callback">数据源委托</param>
        /// <returns></returns>
        public static T Get<T>(string key, string recordCountKey, ref int recordCount, CacheTimeOption cacheTime, 
            RefreshCacheDataWithRefParamHandler<T> callback) where T : class
        {
            // 不缓存时，直接返回
            if (cacheTime == CacheTimeOption.None)
                return callback(ref recordCount);

            
            if (Contains(recordCountKey))
            {
                recordCount = Get<int>(recordCountKey);
            }
            if (Contains(key) && recordCount != 0)
            {
                return Get<T>(key);
            }
            T content = callback(ref recordCount);
            Set(key, content, cacheTime);
            Set(recordCountKey, recordCount, cacheTime);
            return content;
        }

        /// <summary>
        /// 获取指定key缓存数据，如果该缓存不存在，则自动调用数据源委托生成缓存
        /// </summary>
        /// <typeparam name="T">缓存数据的类型</typeparam>
        /// <param name="key">缓存key</param>
        /// <param name="recordCountKey">记录总数的缓存key</param>
        /// <param name="recordCountKey2">记录总数的缓存key2</param>
        /// <param name="recordCount">记录总数(ref)</param>
        /// <param name="recordCount2">记录总数2(ref)</param>
        /// <param name="cacheTime">缓存时间</param>
        /// <param name="callback">数据源委托</param>
        /// <returns></returns>
        public static T Get<T>(string key, string recordCountKey, string recordCountKey2, ref int recordCount, ref int recordCount2, CacheTimeOption cacheTime,
            RefreshCacheDataWithRefParamHandler2<T> callback) where T : class
        {
            // 不缓存时，直接返回
            if (cacheTime == CacheTimeOption.None)
                return callback(ref recordCount, ref recordCount2);


            if (Contains(recordCountKey))
            {
                recordCount = Get<int>(recordCountKey);
            }
            if (Contains(recordCountKey2))
            {
                recordCount2 = Get<int>(recordCountKey2);
            }
            if (Contains(key) && recordCount != 0 && recordCount2 != 0)
            {
                return Get<T>(key);
            }
            T content = callback(ref recordCount, ref recordCount2);
            Set(key, content, cacheTime);
            Set(recordCountKey, recordCount, cacheTime);
            Set(recordCountKey2, recordCount2, cacheTime);
            return content;
        }

        /// <summary>
        /// 获取指定key缓存数据，如果该缓存不存在，则自动调用数据源委托生成缓存
        /// </summary>
        /// <typeparam name="T">缓存数据的类型</typeparam>
        /// <param name="key">缓存key</param>
        /// <param name="recordCountKey">记录总数的缓存key</param>
        /// <param name="recordCount">记录总数(out)</param>
        /// <param name="cacheTime">缓存时间</param>
        /// <param name="callback">数据源委托</param>
        /// <returns></returns>
        public static T Get<T>(string key, string recordCountKey, out int recordCount, CacheTimeOption cacheTime, 
            RefreshCacheDataWithOutParamHandler<T> callback) where T : class
        {
            // 不缓存时，直接返回
            if (cacheTime == CacheTimeOption.None)
                return callback(out recordCount);

            recordCount = 0;
            if (Contains(recordCountKey))
            {
                recordCount = Get<int>(recordCountKey);
            }
            if (Contains(key) && recordCount != 0)
            {
                return Get<T>(key);
            }
            T content = callback(out recordCount);
            Set(key, content, cacheTime);
            Set(recordCountKey, recordCount, cacheTime);
            return content;
        }
        /// <summary>
        /// 获取指定key缓存数据，如果该缓存不存在，则自动调用数据源委托生成缓存(线程安全，但会造成一定性能开销)
        /// </summary>
        /// <typeparam name="T">缓存数据的类型</typeparam>
        /// <param name="key">缓存key</param>
        /// <param name="cacheTime">缓存时间</param>
        /// <param name="callback">数据源委托</param>
        /// <returns></returns>
        public static T GetWithLock<T>(string key, CacheTimeOption cacheTime, RefreshCacheDataHandler<T> callback) where T : class
        {
            // 不缓存时，直接返回
            if (cacheTime == CacheTimeOption.None)
                return callback();
            
            if (Contains(key))
            {
                return Get<T>(key);
            }
            lock (string.Intern(key))
            {
                if (Contains(key))
                {
                    return Get<T>(key);
                }
                else
                {
                    T content = callback();
                    Set(key, content, cacheTime);
                    return content;
                }
            }
        }
        /// <summary>
        /// 获取指定key缓存数据，如果该缓存不存在，则自动调用数据源委托生成缓存(线程安全，但会造成一定性能开销)
        /// </summary>
        /// <typeparam name="T">缓存数据的类型</typeparam>
        /// <param name="key">缓存key</param>
        /// <param name="recordCountKey">记录总数的缓存key</param>
        /// <param name="recordCount">记录总数(ref)</param>
        /// <param name="cacheTime">缓存时间</param>
        /// <param name="callback">数据源委托</param>
        /// <returns></returns>
        public static T GetWithLock<T>(string key, string recordCountKey, ref int recordCount, CacheTimeOption cacheTime, 
            RefreshCacheDataWithRefParamHandler<T> callback) where T : class
        {
            // 不缓存时，直接返回
            if (cacheTime == CacheTimeOption.None)
                return callback(ref recordCount);

            if (Contains(recordCountKey))
            {
                recordCount = Get<int>(recordCountKey);
            }
            if (Contains(key))
            {
                return Get<T>(key);
            }
            lock (string.Intern(key))
            {
                if (Contains(recordCountKey))
                {
                    recordCount = Get<int>(recordCountKey);
                }
                if (Contains(key))
                {
                    return Get<T>(key);
                }
                else
                {
                    T content = callback(ref recordCount);
                    Set(key, content, cacheTime);
                    Set(recordCountKey, recordCount, cacheTime);
                    return content;
                }
            }
        }
        /// <summary>
        /// 获取指定key缓存数据，如果该缓存不存在，则自动调用数据源委托生成缓存(线程安全，但会造成一定性能开销)
        /// </summary>
        /// <typeparam name="T">缓存数据的类型</typeparam>
        /// <param name="key">缓存key</param>
        /// <param name="recordCountKey">记录总数的缓存key</param>
        /// <param name="recordCount">记录总数(out)</param>
        /// <param name="cacheTime">缓存时间</param>
        /// <param name="callback">数据源委托</param>
        /// <returns></returns>
        public static T GetWithLock<T>(string key, string recordCountKey, out int recordCount, CacheTimeOption cacheTime, 
            RefreshCacheDataWithOutParamHandler<T> callback) where T : class
        {
            // 不缓存时，直接返回
            if (cacheTime == CacheTimeOption.None)
                return callback(out recordCount);

            recordCount = 0;
            if (Contains(recordCountKey))
            {
                recordCount = Get<int>(recordCountKey);
            }
            if (Contains(key))
            {
                return Get<T>(key);
            }
            lock (string.Intern(key))
            {
                if (Contains(recordCountKey))
                {
                    recordCount = Get<int>(recordCountKey);
                }
                if (Contains(key))
                {
                    return Get<T>(key);
                }
                else
                {
                    T content = callback(out recordCount);
                    Set(key, content, cacheTime);
                    Set(recordCountKey, recordCount, cacheTime);
                    return content;
                }
            }
        }
        #endregion

        /// <summary>
        /// 从文件缓存依赖获取（目前用于配置文件）
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="key">缓存键</param>
        /// <param name="fileName">文件全名</param>
        /// <param name="cacheSetter">生成返回值的方法</param>
        /// <returns></returns>
        public static T GetByFileDependency<T>(string key, string fileName, Func<T> cacheSetter)
        {
            if (CacheHelper.Contains(key))
                return CacheHelper.Get<T>(key);

            //如果需要文件依赖缓存，需创建依赖对象
            if (fileName.IsNullOrEmpty() || !File.Exists(fileName))
                throw new ArgumentException("读取文件依赖缓存必须指定正确的文件物理路径,并且该文件已存在" + fileName);

            var local = cacheSetter();
            var dep = new CacheDependency(fileName);
            Set(key, local, CacheTimeOption.NotRemovable, CacheExpirationOption.AbsoluteExpiration, dep);
            return local;
        }


        /// <summary>
        /// 生成Key
        /// </summary>
        /// <param name="prefixKey"></param>
        /// <param name="arg0"></param>
        /// <returns></returns>
        public static string RenderKey(string prefixKey, string arg0)
        {
            return string.Format("{0}.{1}", prefixKey, arg0);
        }

        /// <summary>
        /// 生成Key
        /// </summary>
        /// <param name="prefixKey"></param>
        /// <param name="arg0"></param>
        /// <param name="arg1"></param>
        /// <returns></returns>
        public static string RenderKey(string prefixKey, string arg0, string arg1)
        {
            return string.Format("{0}.{1}.{2}", prefixKey, arg0, arg1);
        }

        /// <summary>
        /// 生成Key
        /// </summary>
        /// <param name="prefixKey"></param>
        /// <param name="arg0"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <returns></returns>
        public static string RenderKey(string prefixKey, string arg0, string arg1, string arg2)
        {
            return string.Format("{0}.{1}.{2}.{3}", prefixKey, arg0, arg1, arg2);
        }

        /// <summary>
        /// 生成Key
        /// </summary>
        /// <param name="prefixKey"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string RenderKey(string prefixKey, params string[] args)
        {
            var sb = new StringBuilder(prefixKey);
            for (var i = 0; i < args.Length; i++)
            {
                sb.AppendFormat(".{0}", args[i]);
            }
            return sb.ToString();
        }
    }

    /// <summary>
    /// 过期方式
    /// </summary>
    public enum CacheExpirationOption
    {
        /// <summary>
        /// 绝对过期
        /// </summary>
        AbsoluteExpiration = 0,
        /// <summary>
        /// 弹性过期
        /// </summary>
        SlidingExpiration = 1
    }

    /// <summary>
    /// 常用过期时间
    /// </summary>
    public enum CacheTimeOption
    {
        /// <summary>
        /// 不缓存
        /// </summary>
        None = 0,
        /// <summary>
        /// 短时间 3分钟
        /// </summary>
        Short = 3,
        /// <summary>
        /// 一般正常过期时间 10分钟
        /// </summary>
        Normal = 10,
        /// <summary>
        /// 长时间 30分钟
        /// </summary>
        Long = 30,

        /// <summary>
        /// 永不过期
        /// </summary>
        NotRemovable,

        /// <summary>
        /// 一分钟
        /// </summary>
        OneMinute = 1,
        /// <summary>
        /// 六分钟
        /// </summary>
        SixMinutes = 6,
        /// <summary>
        /// 10分钟
        /// </summary>
        TenMinutes = 10,
        /// <summary>
        /// 十五分钟
        /// </summary>
        QuarterHour = 15,
        /// <summary>
        /// 半小时
        /// </summary>
        HalfHour = 30,
        /// <summary>
        /// 一小时
        /// </summary>
        OneHour = 60,
        /// <summary>
        /// 半天(12小时)
        /// </summary>
        HalfDay = 720
    }
}
