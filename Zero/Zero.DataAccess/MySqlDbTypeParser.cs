using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zero.DataAccess
{
    /// <summary>
    /// Mysql数据库常用的字段类型转换
    /// </summary>
    public static class MySqlDbTypeParser
    {
        /// <summary>
        /// 将Mysql中的表示时间的Unsigned类型转成对应的DataTime
        /// </summary>
        /// <param name="unixDateTime">Mysql中表示时间的Unsigned类型字段值</param>
        /// <returns></returns>
        public static DateTime ConvertUnixDateTime(string unixDateTime)
        {
            if (unixDateTime != null && unixDateTime.Length > 0)
            {
                DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
                long lTime = long.Parse(unixDateTime + "0000000");
                TimeSpan toNow = new TimeSpan(lTime);
                DateTime dtResult = dtStart.Add(toNow);
                return dtResult;
            }
            return DateTime.Now;
        }

        /// <summary>
        /// 将DataTime转成对应的Mysql中的表示时间的Unsigned类型
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string ConvertDateTimeUnix(DateTime dateTime)
        {
            string result = string.Empty;

            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            DateTime dtNow = dateTime;
            TimeSpan toNow = dtNow.Subtract(dtStart);
            result = toNow.Ticks.ToString();
            result = result.Substring(0, result.Length - 7);

            return result;
        }
    }
}
