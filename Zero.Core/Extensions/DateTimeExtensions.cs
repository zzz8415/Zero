using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zero.Core.Extensions
{
    /// <summary>
    /// 日期扩展方法
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// 计算某日起始日期（礼拜一的日期）
        /// </summary>
        /// <param name="source">该周中任意一天</param>
        /// <returns></returns>
        public static DateTime GetMondayDate(this DateTime source)
        {
            int i = source.DayOfWeek - DayOfWeek.Monday;
            // i值 > = 0 ，因为枚举原因，Sunday排在最前，此时Sunday-Monday=-1，必须+7=6。
            if (i == -1) i = 6;
            TimeSpan ts = new TimeSpan(i, 0, 0, 0);
            return source.Subtract(ts);
        }

        /// <summary>
        /// 计算某日起始日期（礼拜日的日期）
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static DateTime GetSundayDate(this DateTime source)
        {
            int i = source.DayOfWeek - DayOfWeek.Sunday;
            if (i != 0) i = 7 - i;// 因为枚举原因，Sunday排在最前，相减间隔要被7减。   
            TimeSpan ts = new TimeSpan(i, 0, 0, 0);
            return source.Add(ts);
        }

        /// <summary>
        /// 转化成标准格式（yyyy-MM-dd HH:mm:ss）
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string ToStandardString(this DateTime source)
        {
            return source.ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// 转化成标准格式（yyyy-MM-dd HH:mm:ss）
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string ToStandardString(this DateTime? source)
        {
            if (source == null)
            {
                return null;
            }
            return source.Value.ToStandardString();
        }

        /// <summary>
        /// 将unix timestamp时间戳(秒) 转换为.NET的DateTime  
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public static DateTime ToUnixDateTimeFromSeconds(this long seconds)
        {
            return DateTimeOffset.FromUnixTimeSeconds(seconds).LocalDateTime;
        }

        /// <summary>
        /// 将unix timestamp时间戳(毫秒) 转换为.NET的DateTime  
        /// </summary>
        /// <param name="milliseconds"></param>
        /// <returns></returns>
        public static DateTime ToUnixDateTimeFromMilliSeconds(this long milliseconds)
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(milliseconds).LocalDateTime;
        }

        /// <summary>
        /// 将.NET的DateTime转换为unix timestamp时间戳(秒)
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static long ToUnixTimeSeconds(this DateTime dateTime)
        {
            return new DateTimeOffset(dateTime).ToUnixTimeSeconds();
        }

        /// <summary>
        /// 将.NET的DateTime转换为unix timestamp时间戳(毫秒)
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static long ToUnixTimeMilliseconds(this DateTime dateTime)
        {
            return new DateTimeOffset(dateTime).ToUnixTimeMilliseconds();
        }

        /// <summary>
        /// 将秒转换成时间字符串
        /// 如果小时>0则显示 HH时mm分ss秒
        /// 否则显示 mm分ss秒
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public static string ToTimeString(this int seconds)
        {
            var time = DateTime.MinValue.AddSeconds(seconds);
            return time.Hour > 0 ? time.ToString("HH时mm分ss秒") : time.ToString("mm分ss秒");
        }
    }
}
