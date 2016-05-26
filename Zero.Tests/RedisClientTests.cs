using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zero.Redis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace Zero.Redis.Tests
{
    [TestClass()]
    public class RedisClientTests
    {
        [TestMethod()]
        public void SetTest()
        {
            string v = "";
            try
            {
                string key = "Test"; // TODO: 初始化为适当的值
                string value = "Value"; // TODO: 初始化为适当的值



                //RedisClient.Instance.Using(x =>
                //    {
                //        x.Set(key, value);
                //        v = x.Get(key);
                //    });
                var client = new RedisClient();
                client.Using(x =>
                {
                    x.HashSet("Zero", key, value);
                    x.HashGet("Zero", key);
                    x.HashGetAll("Zero");
                    
                    v = x.StringGet(key);
                });
            }
            catch (Exception)
            {

                throw;
            }
            Assert.Fail(v);
        }
    }
}
