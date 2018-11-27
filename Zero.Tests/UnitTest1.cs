using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Zero.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var d = ToUnixTimeStamp(DateTime.Now);
            var dd = ToUnixDateTime(d);
            Console.WriteLine(d);
        }

        public DateTime ToUnixDateTime(long timeStamp)
        {
            var now = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(timeStamp);
            return TimeZoneInfo.ConvertTime(now, TimeZoneInfo.Local);
        }

        /// <summary>
        /// 将.NET的DateTime转换为unix timestamp时间戳(秒)
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public long ToUnixTimeStamp(DateTime dateTime)
        {
            return (long)TimeZoneInfo.ConvertTime(dateTime, TimeZoneInfo.Utc).Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        }
    }
}
