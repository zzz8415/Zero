using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using StackExchange.Redis;
using Zero.NETCore.Extensions;
using Microsoft.Extensions.Configuration;
using Zero.NETCore.Attribute;
using Microsoft.Extensions.DependencyInjection;

namespace Zero.NETCore.Redis
{
    /// <summary>
    /// Redis客户端,提供Get,Set及Remove方法
    /// </summary>
    [Inject(OptionsLifetime = ServiceLifetime.Singleton)]
    public class RedisClient : IDisposable
    {
        /// <summary>
        /// Redis客户端
        /// </summary>
        protected IDatabase Client
        {
            get
            {
                return channel.GetDatabase();
            }
        }


        private ConnectionMultiplexer channel = null;

        /// <summary>
        /// 实例化Redis链接
        /// </summary>
        /// <param name="configuration"></param>
        public RedisClient(IConfiguration configuration)
        {
            var config = configuration.GetSection("RedisConfig").Get<RedisConfig>();
            var options = new ConfigurationOptions
            {
                Password = config.Password,
                KeepAlive = config.KeepAlive,
                ConnectRetry = config.ConnectRetry,
                AbortOnConnectFail = false,
                ConnectTimeout = config.ConnectTimeout,
                DefaultDatabase = config.DBRegion,
                SyncTimeout = config.SyncTimeout
            };

            options.EndPoints.Add(config.Host, config.Port);

            this.channel = ConnectionMultiplexer.Connect(options);
        }

        /// <summary>
        /// 存入Redis，使用默认的JsonSerializer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public bool Set<T>(string key, T value)
        {
            return this.Client.StringSet(key, value.ToJson());
        }

        /// <summary>
        /// 存入Redis
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
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
            return this.Client.StringSet(key, value);
        }

        /// <summary>
        /// 存入Redis，并指定过期时间，使用默认的JsonSerializer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiresIn"></param>
        public bool Set<T>(string key, T value, TimeSpan expiresIn)
        {
            return this.Client.StringSet(key, value.ToJson(), expiresIn);
        }

        /// <summary>
        /// 存入Redis
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiresIn"></param>
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
            return this.Client.StringSet(key, value, expiresIn);
        }

        /// <summary>
        /// 从Redis读取，使用默认的JsonSerializer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T Get<T>(string key) where T : class
        {
            var value = this.Client.StringGet(key);
            return value.IsNullOrEmpty ? default(T) : JsonExtensions.DeserializeJson<T>(value);
        }

        /// <summary>
        /// 从Redis读取
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
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
            return this.Client.StringGet(key);
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
            var values = this.Client.StringGet(redisKeys);
            var dic = new Dictionary<string, T>();
            for (var i = 0; i < values.Length; i++)
            {
                var value = values[i];
                dic[redisKeys[i]] = value.IsNullOrEmpty ? default(T) : JsonExtensions.DeserializeJson<T>(values[i]);
            }
            return dic;
        }

        /// <summary>
        /// 根据Key数组从Redis读取
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keys"></param>
        /// <returns></returns>
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
            var values = this.Client.StringGet(redisKeys);
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
            return this.Client.StringSet(dic.ToArray());
        }

        /// <summary>
        /// 按参数指定的键值对，逐一存入Redis，
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keyValues"></param>
        public bool Set<T>(Dictionary<string, T> keyValues)
        {
            var dic = new Dictionary<RedisKey, RedisValue>();
            foreach (var i in keyValues)
            {
                dic[i.Key] = i.Value.ToJson();
            }
            return this.Client.StringSet(dic.ToArray());
        }

        /// <summary>
        /// 按参数指定的键值对，逐一存入Redis，使用Protobuf序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keyValues"></param>
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
        public void Using(Action<IDatabase> action)
        {
            action(this.Client);
        }

        /// <summary>
        /// 调用客户端
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        /// <returns></returns>
        public T Using<T>(Func<IDatabase, T> func)
        {
            return func(this.Client);
        }

        /// <summary>
        /// 调用连接渠道
        /// </summary>
        /// <param name="action"></param>
        public void UsingChannel(Action<ConnectionMultiplexer> action)
        {
            action(this.channel);
        }

        /// <summary>
        /// 调用连接渠道
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        /// <returns></returns>
        public T UsingChannel<T>(Func<ConnectionMultiplexer, T> func)
        {
            return func(this.channel);
        }

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
            this.Client.KeyDelete(key);
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

            this.Client.KeyDelete(redisKeys);
        }

        /// <summary>
        /// 生成RedisKey
        /// </summary>
        /// <param name="prefixKey"></param>
        /// <param name="arg0"></param>
        /// <returns></returns>
        public string RenderRedisKey(string prefixKey, string arg0)
        {
            return string.Format("{0}.{1}", prefixKey, arg0);
        }

        /// <summary>
        /// 生成RedisKey
        /// </summary>
        /// <param name="prefixKey"></param>
        /// <param name="arg0"></param>
        /// <param name="arg1"></param>
        /// <returns></returns>
        public string RenderRedisKey(string prefixKey, string arg0, string arg1)
        {
            return string.Format("{0}.{1}.{2}", prefixKey, arg0, arg1);
        }

        /// <summary>
        /// 生成RedisKey
        /// </summary>
        /// <param name="prefixKey"></param>
        /// <param name="arg0"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <returns></returns>
        public string RenderRedisKey(string prefixKey, string arg0, string arg1, string arg2)
        {
            return string.Format("{0}.{1}.{2}.{3}", prefixKey, arg0, arg1, arg2);
        }

        /// <summary>
        /// 生成RedisKey
        /// </summary>
        /// <param name="prefixKey"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public string RenderRedisKey(string prefixKey, params string[] args)
        {
            var sb = new StringBuilder(prefixKey);
            for (var i = 0; i < args.Length; i++)
            {
                sb.AppendFormat(".{0}", args[i]);
            }
            return sb.ToString();
        }

        ///// <summary>
        ///// 还原Key中包含的参数
        ///// </summary>
        ///// <param name="key"></param>
        ///// <param name="prefixKey"></param>
        ///// <returns></returns>
        //public string RestoreParams(string key, string prefixKey)
        //{
        //    if (key.Length <= prefixKey.Length)
        //    {
        //        return string.Empty;
        //    }
        //    return key.Substring(prefixKey.Length + 1);
        //}

        ///// <summary>
        ///// 还原Key中包含的参数
        ///// </summary>
        ///// <param name="key"></param>
        ///// <param name="prefixKey"></param>
        ///// <param name="arg0"></param>
        ///// <returns></returns>
        //public string RestoreParams(string key, string prefixKey, string arg0)
        //{
        //    var pKey = RenderRedisKey(prefixKey, arg0);
        //    return RestoreParams(key, pKey);
        //}

        ///// <summary>
        ///// 还原Key中包含的参数
        ///// </summary>
        ///// <param name="key"></param>
        ///// <param name="prefixKey"></param>
        ///// <param name="arg0"></param>
        ///// <param name="arg1"></param>
        ///// <returns></returns>
        //public string RestoreParams(string key, string prefixKey, string arg0, string arg1)
        //{
        //    var pKey = RenderRedisKey(prefixKey, arg0, arg1);
        //    return RestoreParams(key, pKey);
        //}

        ///// <summary>
        ///// 还原Key中包含的参数
        ///// </summary>
        ///// <param name="key"></param>
        ///// <param name="prefixKey"></param>
        ///// <param name="arg0"></param>
        ///// <param name="arg1"></param>
        ///// <param name="arg2"></param>
        ///// <returns></returns>
        //public string RestoreParams(string key, string prefixKey, string arg0, string arg1, string arg2)
        //{
        //    var pKey = RenderRedisKey(prefixKey, arg0, arg1, arg2);
        //    return RestoreParams(key, pKey);
        //}

        ///// <summary>
        ///// 还原Key中包含的参数
        ///// </summary>
        ///// <param name="key"></param>
        ///// <param name="prefixKey"></param>
        ///// <param name="args"></param>
        ///// <returns></returns>
        //public string RestoreParams(string key, string prefixKey, params string[] args)
        //{
        //    var pKey = RenderRedisKey(prefixKey, args);
        //    return RestoreParams(key, pKey);
        //}

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)。
                    if (this.channel != null)
                    {
                        this.channel.Close();
                    }
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。

                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~RedisClient() {
        //   // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
        //   Dispose(false);
        // }

        // 添加此代码以正确实现可处置模式。
        /// <summary>
        /// 释放连接
        /// </summary>
        public void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            // TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
