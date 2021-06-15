using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zero.NETCore;
using System;
using System.Collections.Generic;
using System.Text;
using Zero.NETCore.Inject;
using System.Diagnostics;
using FreeRedis;

namespace Zero.NETCore.Tests
{
    [TestClass()]
    public class LogClientTests
    {
        [TestMethod()]
        public void WriteInfoTest()
        {
            var client = new LogClient();
            try
            {
                client.WriteInfo("WriteInfo");
                client.WriteInfo("WriteInfo");
                client.WriteTrace("WriteTrace");
                client.WriteTrace("WriteTrace");
                client.WriteCustom("WriteCustom", "WriteCustom");
                client.WriteCustom("WriteCustom", "WriteCustom");
                client.WriteDebug("WriteDebug");
                client.WriteDebug("WriteDebug");
                client.WriteError("WriteError");
                client.WriteError("WriteError");
                client.WriteFatal("WriteFatal");
                client.WriteFatal("WriteFatal");
                client.WriteException(new Exception("111"));
                client.WriteException(new Exception("222"));
                var y = 0;
                var x = 22 / y;
            }
            catch (Exception ex)
            {
                client.WriteException(ex);
            }

            Assert.Fail();
        }


        [TestMethod()]
        public void RedisTest()
        {
            var timer = new Stopwatch();
            var redisKey = $"Geekee.CoinBot.TacticInvest.10001";
            //var cst = "127.0.0.1:6379,defaultDatabase=0";
            var cst = "47.244.91.220:63791,password=Geekee2018,defaultDatabase=0";
            timer.Start();
            timer.Stop();


            for (var i = 0; i < 5; i++)
            {
                timer.Reset();
                timer.Start();
                var client = new RedisClient(cst);
                var ts = client.HKeys(redisKey);
                timer.Stop();
                Console.WriteLine(timer.Elapsed);
                new LogClient().WriteInfo(i + " 1 " + timer.Elapsed.ToString());

                timer.Reset();
                timer.Start();
                var client2 = StackExchange.Redis.ConnectionMultiplexer.Connect(cst);
                var ts2 = client2.GetDatabase().HashKeys(redisKey);
                timer.Stop();
                new LogClient().WriteInfo(i + " 2 " + timer.Elapsed.ToString());

                timer.Reset();
                timer.Start();
                var client3 = new CSRedis.CSRedisClient(cst);
                var ts3 = client.HKeys(redisKey);
                timer.Stop();
                Console.WriteLine(timer.Elapsed);
                new LogClient().WriteInfo(i + " 3 " + timer.Elapsed.ToString());
            }
            Assert.Fail();
        }
    }
}