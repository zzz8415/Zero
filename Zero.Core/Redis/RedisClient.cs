using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using StackExchange.Redis;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Zero.Core.Attribute;
using Zero.Core.Extensions;
using Zero.Core.Inject;

namespace Zero.Core.Redis
{
    /// <summary>
    /// Redis客户端,提供Get,Set及Remove方法
    /// </summary>
    [Inject(OptionsLifetime = ServiceLifetime.Singleton)]
    public class RedisClient(WebConfig webConfig)
    {
        /// <summary>
        /// Redis Database
        /// </summary>
        public IDatabase Database => Multiplexer.GetDatabase();

        /// <summary>
        /// Multiplexer
        /// </summary>
        public ConnectionMultiplexer Multiplexer => ConnectionMultiplexer.Connect(webConfig.Configuration.GetConnectionString("redis"));


        /// <summary>
        /// 存入Redis，使用默认的JsonSerializer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public bool Set<T>(string key, T value) where T : class
        {
            return this.Database.StringSet(key, value.ToJson());
        }

        ///// <summary>
        ///// 存入Redis
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="key"></param>
        ///// <param name="value"></param>
        //public bool SetProtobuf<T>(string key, T value)
        //{
        //    return this.Client.StringSet(key, value.ToProtoBuf());
        //}

        /// <summary>
        /// 存入Redis
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public bool Set(string key, string value)
        {
            return this.Database.StringSet(key, value);
        }

        /// <summary>
        /// 存入Redis，并指定过期时间，使用默认的JsonSerializer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiresIn"></param>
        public bool Set<T>(string key, T value, TimeSpan expiresIn) where T : class
        {
            return this.Database.StringSet(key, value.ToJson(), expiresIn);
        }

        ///// <summary>
        ///// 存入Redis
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="key"></param>
        ///// <param name="value"></param>
        ///// <param name="expiresIn"></param>
        //public bool SetProtobuf<T>(string key, T value, TimeSpan expiresIn)
        //{
        //    return this.Client.StringSet(key, value.ToProtoBuf(), expiresIn);
        //}

        /// <summary>
        /// 存入Redis
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiresIn"></param>
        public bool Set(string key, string value, TimeSpan expiresIn)
        {
            return this.Database.StringSet(key, value, expiresIn);
        }

        /// <summary>
        /// 从Redis读取，使用默认的JsonSerializer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T Get<T>(string key) where T : class
        {
            var value = this.Database.StringGet(key);
            return value.IsNullOrEmpty ? default : JsonExtensions.DeserializeJson<T>(value);
        }

        ///// <summary>
        ///// 从Redis读取
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="key"></param>
        ///// <returns></returns>
        //public T GetProtobuf<T>(string key) where T : class
        //{
        //    var value = this.Client.StringGet(key);
        //    return value.IsNullOrEmpty ? default(T) : ProtoBufExtensions.DeserializeProtoBuf<T>(value);
        //}

        /// <summary>
        /// 从Redis读取
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string Get(string key)
        {
            return this.Database.StringGet(key);
        }

        /// <summary>
        /// 根据Key数组从Redis读取，使用默认的JsonSerializer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keys"></param>
        /// <returns></returns>
        public IDictionary<string, T> Get<T>(List<string> keys) where T : class
        {
            var redisKeys = new RedisKey[keys.Count];
            for (var i = 0; i < keys.Count; i++)
            {
                redisKeys[i] = keys[i];
            }
            var values = this.Database.StringGet(redisKeys);
            var dic = new Dictionary<string, T>();
            for (var i = 0; i < values.Length; i++)
            {
                var value = values[i];
                dic[redisKeys[i]] = value.IsNullOrEmpty ? default : JsonExtensions.DeserializeJson<T>(values[i]);
            }
            return dic;
        }

        ///// <summary>
        ///// 根据Key数组从Redis读取
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="keys"></param>
        ///// <returns></returns>
        //public IDictionary<string, T> GetProtobuf<T>(List<string> keys) where T : class
        //{
        //    var redisKeys = new RedisKey[keys.Count];
        //    for (var i = 0; i < keys.Count; i++)
        //    {
        //        redisKeys[i] = keys[i];
        //    }
        //    var values = this.Client.StringGet(redisKeys);
        //    var dic = new Dictionary<string, T>();
        //    for (var i = 0; i < values.Length; i++)
        //    {
        //        var value = values[i];
        //        dic[redisKeys[i]] = value.IsNullOrEmpty ? default(T) : ProtoBufExtensions.DeserializeProtoBuf<T>(values[i]);
        //    }
        //    return dic;
        //}

        /// <summary>
        /// 根据Key数组从Redis读取
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public Dictionary<string, string> Get(List<string> keys)
        {
            var redisKeys = new RedisKey[keys.Count];
            for (var i = 0; i < keys.Count; i++)
            {
                redisKeys[i] = keys[i];
            }
            var values = this.Database.StringGet(redisKeys);
            var dic = new Dictionary<string, string>();
            for (var i = 0; i < values.Length; i++)
            {
                dic[redisKeys[i]] = values[i];
            }
            return dic;
        }

        /// <summary>
        /// 按参数指定的键值对，逐一存入Redis，
        /// </summary>
        /// <param name="keyValues"></param>
        public bool Set(Dictionary<string, string> keyValues)
        {
            var dic = new Dictionary<RedisKey, RedisValue>();
            foreach (var i in keyValues)
            {
                dic[i.Key] = i.Value;
            }
            return this.Database.StringSet([.. dic]);
        }

        /// <summary>
        /// 按参数指定的键值对，逐一存入Redis，
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keyValues"></param>
        public bool Set<T>(Dictionary<string, T> keyValues) where T : class
        {
            var dic = new Dictionary<RedisKey, RedisValue>();
            foreach (var i in keyValues)
            {
                dic[i.Key] = i.Value.ToJson();
            }
            return this.Database.StringSet([.. dic]);
        }

        ///// <summary>
        ///// 按参数指定的键值对，逐一存入Redis，使用Protobuf序列化
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="keyValues"></param>
        //public bool SetProtobuf<T>(Dictionary<string, T> keyValues)
        //{
        //    var dic = new Dictionary<RedisKey, RedisValue>();
        //    foreach (var i in keyValues)
        //    {
        //        dic[i.Key] = i.Value.ToProtoBuf();
        //    }
        //    return this.Client.StringSet(dic.ToArray());
        //}

        /// <summary>
        /// 调用客户端
        /// </summary>
        /// <param name="action"></param>
        //public void Using(Action<IDatabase> action)
        //{
        //    action(this.Client);
        //}

        /// <summary>
        /// 调用客户端
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        /// <returns></returns>
        //public T Using<T>(Func<IDatabase, T> func)
        //{
        //    return func(this.Client);
        //}

        /// <summary>
        /// 调用连接渠道
        /// </summary>
        /// <param name="action"></param>
        //public void UsingChannel(Action<ConnectionMultiplexer> action)
        //{
        //    action(_multiplexer);
        //}

        /// <summary>
        /// 调用连接渠道
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        /// <returns></returns>
        //public T UsingChannel<T>(Func<ConnectionMultiplexer, T> func)
        //{
        //    return func(_multiplexer);
        //}

        ///// <summary>
        ///// 移除指定的key,并返回移除的条数（isPrefix为true，表示所有key为前缀的记录都移除）
        ///// </summary>
        ///// <param name="key"></param>
        ///// <param name="isPrefix"></param>
        ///// <returns></returns>
        //public int Remove(string key, bool isPrefix = false)
        //{
        //    int cnt = 0;
        //    if (isPrefix)
        //    {
        //        #region 删除指定前缀的所有记录
        //        List<string> allKeys = this.client.get();
        //        allKeys.Sort((a, b) => string.Compare(a, b, StringComparison.OrdinalIgnoreCase));
        //        var find = false;
        //        foreach (string s in allKeys)
        //        {
        //            if (s.StartsWith(key, StringComparison.OrdinalIgnoreCase))
        //            {
        //                if (!find)
        //                    Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " 开始第一个删除");
        //                this.client.KeyDelete(s);
        //                find = true;
        //                cnt++;
        //            }
        //            else if (find)
        //            {
        //                // 找到记录，然后又变成没找到，说明后续都不用循环了（前面排序了）
        //                break;
        //            }
        //        }
        //        #endregion
        //    }
        //    else
        //    {
        //        if (this.client.KeyExists(key))
        //        {
        //            cnt++;
        //            this.client.KeyDelete(key);
        //        }
        //    }
        //    return cnt;
        //}

        /// <summary>
        /// 移除Redis key
        /// </summary>
        /// <param name="key"></param>
        public void Remove(string key)
        {
            this.Database .KeyDelete(key);
        }

        /// <summary>
        /// 移除Redis key
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public void Remove(List<string> keys)
        {
            var redisKeys = new RedisKey[keys.Count];
            for (var i = 0; i < keys.Count; i++)
            {
                redisKeys[i] = keys[i];
            }

            this.Database.KeyDelete(redisKeys);
        }

        /// <summary>
        /// 生成RedisKey
        /// </summary>
        /// <param name="prefixKey"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public string RenderKey(string prefixKey, params string[] args)
        {
            return $"{prefixKey}:{string.Join(":", args)}";
        }

    }
}
